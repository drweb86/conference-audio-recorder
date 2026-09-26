using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using HDE.Platform.Logging;
using NAudio.Wave;
using NAudio.Wave.Alsa;

namespace ConferenceAudioRecorder.Services;

[SupportedOSPlatform("linux")]
internal sealed class LinuxAudioBackend : IAudioBackend, IDisposable
{
    private readonly ILog _log;
    private readonly Timer _poll;
    private string _signature = string.Empty;

    public LinuxAudioBackend(ILog log)
    {
        _log = log;
        _signature = Signature();
        _poll = new Timer(_ => Poll(), null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
    }

    public event EventHandler DevicesChanged;

    public IReadOnlyList<string> GetInputDevices()
    {
        return SafeCaptureDevices()
            .Where(device => !IsMonitor(device))
            .Select(Format)
            .ToList();
    }

    public IReadOnlyList<string> GetOutputDevices()
    {
        var captures = SafeCaptureDevices();
        var monitors = captures.Where(IsMonitor).Select(Format).ToList();
        if (monitors.Count > 0)
            return monitors;

        return SafePlaybackDevices().Select(Format).ToList();
    }

    public string GetDefaultInputDevice()
    {
        return Prefer(GetInputDevices(), "default", "sysdefault", "pulse", "pipewire");
    }

    public string GetDefaultOutputDevice()
    {
        return Prefer(GetOutputDevices(), "default", "sysdefault", "pulse", "pipewire", "monitor");
    }

    public ICaptureSession StartInputCapture(string deviceName, int sampleRate, int channels)
    {
        return new LinuxCaptureSession(IdOf(deviceName), sampleRate, channels, _log);
    }

    public ICaptureSession StartOutputCapture(string deviceName, int sampleRate, int channels)
    {
        var pcm = ResolveMonitor(IdOf(deviceName));
        _log.Debug($"Speaker capture uses ALSA device '{pcm}' for '{deviceName}'.");
        return new LinuxCaptureSession(pcm, sampleRate, channels, _log);
    }

    public void Dispose()
    {
        _poll.Dispose();
    }

    private void Poll()
    {
        try
        {
            var signature = Signature();
            if (signature == _signature)
                return;

            _signature = signature;
            DevicesChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _log.Error(ex);
        }
    }

    private string Signature()
    {
        return string.Join("|", GetInputDevices()) + "#" + string.Join("|", GetOutputDevices());
    }

    private string ResolveMonitor(string selectedId)
    {
        var captures = SafeCaptureDevices();
        if (captures.Any(device => device.Name == selectedId))
            return selectedId;

        var playback = SafePlaybackDevices().FirstOrDefault(device => device.Name == selectedId);
        var monitors = captures.Where(IsMonitor).ToList();
        if (playback != null && monitors.Count > 0)
        {
            var description = FirstLine(playback.Description);
            var matched = monitors.FirstOrDefault(device =>
                !string.IsNullOrWhiteSpace(description) &&
                device.Description.Contains(description, StringComparison.OrdinalIgnoreCase));
            if (matched != null)
                return matched.Name;
        }

        if (monitors.Count == 1)
            return monitors[0].Name;

        return selectedId.EndsWith(".monitor", StringComparison.OrdinalIgnoreCase)
            ? selectedId
            : selectedId + ".monitor";
    }

    private bool _loggedEnumerationError;

    private List<AlsaDeviceInfo> SafeCaptureDevices()
    {
        return SafeEnumerate(AlsaDeviceEnumerator.GetCaptureDevices);
    }

    private List<AlsaDeviceInfo> SafePlaybackDevices()
    {
        return SafeEnumerate(AlsaDeviceEnumerator.GetPlaybackDevices);
    }

    private List<AlsaDeviceInfo> SafeEnumerate(Func<IReadOnlyList<AlsaDeviceInfo>> enumerate)
    {
        try
        {
            return enumerate().ToList();
        }
        catch (Exception ex)
        {
            if (!_loggedEnumerationError)
            {
                _loggedEnumerationError = true;
                _log.Error(ex);
            }

            return new List<AlsaDeviceInfo>();
        }
    }

    private static bool IsMonitor(AlsaDeviceInfo device)
    {
        return ContainsMonitor(device.Name) || ContainsMonitor(device.Description);
    }

    private static bool ContainsMonitor(string value)
    {
        return value != null && value.Contains("monitor", StringComparison.OrdinalIgnoreCase);
    }

    private static string Format(AlsaDeviceInfo device)
    {
        var description = FirstLine(device.Description);
        if (string.IsNullOrWhiteSpace(description) || description == device.Name)
            return device.Name;

        return $"{description} ({device.Name})";
    }

    private static string IdOf(string formatted)
    {
        if (formatted != null && formatted.EndsWith(')'))
        {
            var open = formatted.LastIndexOf(" (", StringComparison.Ordinal);
            if (open >= 0)
                return formatted.Substring(open + 2, formatted.Length - open - 3);
        }

        return formatted;
    }

    private static string FirstLine(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var line = value.Split('\n')[0].Trim();
        return line;
    }

    private static string Prefer(IReadOnlyList<string> devices, params string[] hints)
    {
        foreach (var hint in hints)
        {
            var match = devices.FirstOrDefault(device => MatchesHint(IdOf(device), hint));
            if (match != null)
                return match;
        }

        return devices.FirstOrDefault();
    }

    private static bool MatchesHint(string id, string hint)
    {
        if (string.IsNullOrEmpty(id))
            return false;

        if (hint == "monitor")
            return id.Contains("monitor", StringComparison.OrdinalIgnoreCase);

        return id.Equals(hint, StringComparison.OrdinalIgnoreCase)
            || id.StartsWith(hint + ":", StringComparison.OrdinalIgnoreCase);
    }

    private sealed class LinuxCaptureSession : ICaptureSession
    {
        private readonly AlsaIn _capture;
        private readonly WaveFileWriter _writer;
        private readonly string _waveFile;

        public LinuxCaptureSession(string pcmName, int sampleRate, int channels, ILog log)
        {
            _waveFile = Path.GetTempFileName();
            if (File.Exists(_waveFile))
                File.Delete(_waveFile);

            (_capture, _writer) = Open(pcmName, _waveFile, sampleRate, channels, log);
        }

        public string WaveFile => _waveFile;

        public void Stop()
        {
            try
            {
                _capture.StopRecording();
                _writer.Flush();
            }
            finally
            {
                _writer.Dispose();
                _capture.Dispose();
            }

            WaveNormalizer.Normalize(_waveFile);
        }

        private static (AlsaIn Capture, WaveFileWriter Writer) Open(string pcmName, string waveFile, int sampleRate, int channels, ILog log)
        {
            var candidates = new List<string> { PlugName(pcmName) };
            if (!string.Equals(candidates[0], pcmName, StringComparison.Ordinal))
                candidates.Add(pcmName);

            Exception last = null;
            foreach (var device in candidates.Distinct())
            {
                foreach (var format in FormatsToTry(sampleRate, channels))
                {
                    var input = new AlsaIn(device);
                    input.WaveFormat = format;
                    WaveFileWriter writer = null;
                    try
                    {
                        writer = new WaveFileWriter(waveFile, format);
                        var writes = 0;
                        input.DataAvailable += (_, args) =>
                        {
                            writer.Write(args.BufferSpan);
                            if (++writes % 100 == 0)
                                writer.Flush();
                        };
                        input.StartRecording();
                        log.Debug($"Opened ALSA capture '{device}' as {format.SampleRate} Hz, {format.BitsPerSample}-bit, {format.Channels} ch.");
                        return (input, writer);
                    }
                    catch (Exception ex)
                    {
                        last = ex;
                        log.Debug($"ALSA capture '{device}' rejected {format}: {ex.Message}");
                        writer?.Dispose();
                        input.Dispose();
                        if (File.Exists(waveFile))
                            File.Delete(waveFile);
                    }
                }
            }

            throw new InvalidOperationException($"Cannot open ALSA capture device '{pcmName}'.", last);
        }

        private static List<WaveFormat> FormatsToTry(int sampleRate, int channels)
        {
            var formats = new List<WaveFormat>
            {
                new WaveFormat(sampleRate, 16, channels),
                new WaveFormat(48000, 16, 2),
                new WaveFormat(44100, 16, 2),
                new WaveFormat(48000, 16, 1),
                new WaveFormat(44100, 16, 1)
            };

            var unique = new List<WaveFormat>();
            foreach (var format in formats)
            {
                var seen = false;
                foreach (var existing in unique)
                {
                    if (existing.SampleRate == format.SampleRate &&
                        existing.BitsPerSample == format.BitsPerSample &&
                        existing.Channels == format.Channels)
                    {
                        seen = true;
                        break;
                    }
                }

                if (!seen)
                    unique.Add(format);
            }

            return unique;
        }

        private static string PlugName(string pcmName)
        {
            if (pcmName != null && pcmName.StartsWith("hw:", StringComparison.Ordinal))
                return "plug" + pcmName;

            return pcmName;
        }
    }
}
