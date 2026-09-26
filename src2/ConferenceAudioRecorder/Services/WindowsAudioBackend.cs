using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using HDE.Platform.Logging;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace ConferenceAudioRecorder.Services;

[SupportedOSPlatform("windows")]
internal sealed class WindowsAudioBackend : IAudioBackend
{
    private readonly ILog _log;
    private readonly MMDeviceEnumerator _devices = new();
    private readonly MMDeviceNotificationClient _notifications;

    public WindowsAudioBackend(ILog log)
    {
        _log = log;
        _notifications = _devices.CreateNotificationClient(useSynchronizationContext: false);
        _notifications.DeviceAdded += (_, _) => DevicesChanged?.Invoke(this, EventArgs.Empty);
        _notifications.DeviceRemoved += (_, _) => DevicesChanged?.Invoke(this, EventArgs.Empty);
        _notifications.DeviceStateChanged += (_, _) => DevicesChanged?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler DevicesChanged;

    public IReadOnlyList<string> GetInputDevices()
    {
        return ListDevices(DataFlow.Capture);
    }

    public IReadOnlyList<string> GetOutputDevices()
    {
        return ListDevices(DataFlow.Render);
    }

    public string GetDefaultInputDevice()
    {
        return DefaultDevice(DataFlow.Capture, Role.Communications);
    }

    public string GetDefaultOutputDevice()
    {
        return DefaultDevice(DataFlow.Render, Role.Multimedia);
    }

    public ICaptureSession StartInputCapture(string deviceName, int sampleRate, int channels)
    {
        _log.Debug($"Microphone capture keeps the Windows device format, then converts to {sampleRate} Hz, {channels} ch.");
        var device = FindDevice(DataFlow.Capture, deviceName);
        return new WindowsCaptureSession(device, loopback: false);
    }

    public ICaptureSession StartOutputCapture(string deviceName, int sampleRate, int channels)
    {
        _log.Debug($"Speaker capture keeps the Windows device format, then converts to {sampleRate} Hz, {channels} ch.");
        var device = FindDevice(DataFlow.Render, deviceName);
        return new WindowsCaptureSession(device, loopback: true);
    }

    public void Dispose()
    {
        _notifications.Dispose();
        _devices.Dispose();
    }

    private List<string> ListDevices(DataFlow flow)
    {
        _log.Debug($"Get list of {flow} devices");
        var names = new List<string>();
        foreach (MMDevice device in _devices.EnumerateAudioEndPoints(flow, DeviceState.Active))
        {
            names.Add(device.FriendlyName);
            _log.Debug($"{flow} device {device.FriendlyName}");
        }

        return names;
    }

    private string DefaultDevice(DataFlow flow, Role role)
    {
        try
        {
            if (!_devices.HasDefaultAudioEndpoint(flow, role))
            {
                _log.Debug($"Default {flow} endpoint is missing");
                return null;
            }

            var device = _devices.GetDefaultAudioEndpoint(flow, role);
            _log.Debug($"Default {flow} device is {device.FriendlyName}");
            return device.FriendlyName;
        }
        catch (Exception e)
        {
            _log.Error(e);
            return null;
        }
    }

    private MMDevice FindDevice(DataFlow flow, string friendlyName)
    {
        foreach (MMDevice device in _devices.EnumerateAudioEndPoints(flow, DeviceState.Active))
        {
            if (device.FriendlyName == friendlyName)
                return device;
        }

        throw new InvalidOperationException($"Audio device was not found: {friendlyName}");
    }

    private sealed class WindowsCaptureSession : ICaptureSession
    {
        private readonly WasapiRecorder _recorder;
        private readonly WasapiPlayer _silence;
        private readonly WaveFileWriter _writer;
        private readonly string _waveFile;
        private int _writes;
        private bool _cleaned;

        public WindowsCaptureSession(MMDevice device, bool loopback)
        {
            _waveFile = Path.GetTempFileName();
            if (File.Exists(_waveFile))
                File.Delete(_waveFile);

            var builder = new WasapiRecorderBuilder()
                .WithDevice(device)
                .WithBufferLength(50);
            if (loopback)
                builder.WithLoopbackCapture();

            _recorder = builder.Build();
            _writer = new WaveFileWriter(_waveFile, _recorder.WaveFormat);
            _recorder.DataAvailable += OnData;

            if (loopback)
            {
                _silence = new WasapiPlayerBuilder()
                    .WithDevice(device)
                    .Build();
                _silence.Init(new SilenceProvider(new WaveFormat(48000, 16, 2)));
                _silence.Play();
            }

            try
            {
                _recorder.StartRecording();
            }
            catch
            {
                CleanupCapture();
                throw;
            }
        }

        public string WaveFile => _waveFile;

        public void Stop()
        {
            try
            {
                _recorder.StopRecording();
                _writer.Flush();
            }
            finally
            {
                CleanupCapture();
            }

            WaveNormalizer.Normalize(_waveFile);
        }

        private void OnData(ReadOnlySpan<byte> buffer, AudioClientBufferFlags flags, long devicePosition, long qpcPosition)
        {
            _writer.Write(buffer);
            if (++_writes % 100 == 0)
                _writer.Flush();
        }

        private void CleanupCapture()
        {
            if (_cleaned)
                return;

            _cleaned = true;
            _writer.Dispose();
            _silence?.Stop();
            _silence?.Dispose();
            _recorder.Dispose();
        }
    }
}
