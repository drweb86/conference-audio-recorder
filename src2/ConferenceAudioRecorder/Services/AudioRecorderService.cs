using System;
using System.IO;
using System.Linq;
using HDE.Platform.Logging;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace ConferenceAudioRecorder.Services;

internal class AudioRecorderService
{
    private readonly ILog _log;
    private readonly IAudioBackend _audio;
    private DateTime _recordingStarted;
    private DateTime _recordingEnded;
    private ICaptureSession _speaker;
    private ICaptureSession _microphone;

    public AudioRecorderService(ILog log, IAudioBackend audio)
    {
        _log = log;
        _audio = audio;
    }

    public bool IsAudioRecording => _microphone != null || _speaker != null;

    public void StartRecording(string inputDeviceFriendlyName, string outputDeviceFriendlyName)
    {
        _log.Debug($"Start recording input {inputDeviceFriendlyName}, output {outputDeviceFriendlyName}");

        if (outputDeviceFriendlyName != null)
            _speaker = _audio.StartOutputCapture(outputDeviceFriendlyName);

        try
        {
            if (inputDeviceFriendlyName != null)
                _microphone = _audio.StartInputCapture(inputDeviceFriendlyName);
        }
        catch
        {
            StopSession(ref _speaker);
            throw;
        }

        _recordingStarted = DateTime.Now;
    }

    public void StopRecording(string folderName)
    {
        _recordingEnded = DateTime.Now;
        string mixWave = null;
        try
        {
            _microphone?.Stop();
            _speaker?.Stop();

            if (!Directory.Exists(folderName))
                Directory.CreateDirectory(folderName);

            var microphoneFile = ExistingWave(_microphone);
            var speakerFile = ExistingWave(_speaker);
            if (microphoneFile == null && speakerFile == null)
                return;

            if (microphoneFile != null && speakerFile != null)
            {
                ConvertWaveToMp3(
                    microphoneFile,
                    FileNameGenerator.GetOutputMp3FileName(_recordingStarted, _recordingEnded, folderName, " - microphone"));
                ConvertWaveToMp3(
                    speakerFile,
                    FileNameGenerator.GetOutputMp3FileName(_recordingStarted, _recordingEnded, folderName, " - speaker"));

                mixWave = Path.GetTempFileName();
                MixFiles(microphoneFile, speakerFile, mixWave);
                ConvertWaveToMp3(
                    mixWave,
                    FileNameGenerator.GetOutputMp3FileName(_recordingStarted, _recordingEnded, folderName));
            }
            else
            {
                ConvertWaveToMp3(
                    microphoneFile ?? speakerFile,
                    FileNameGenerator.GetOutputMp3FileName(_recordingStarted, _recordingEnded, folderName));
            }
        }
        catch (Exception e)
        {
            _log.Error(e);
            throw;
        }
        finally
        {
            DeleteSessionFile(ref _microphone);
            DeleteSessionFile(ref _speaker);
            if (mixWave != null && File.Exists(mixWave))
                File.Delete(mixWave);
        }
    }

    private static string ExistingWave(ICaptureSession session)
    {
        if (session?.WaveFile == null || !File.Exists(session.WaveFile))
            return null;

        return session.WaveFile;
    }

    private void ConvertWaveToMp3(string sourceWaveFile, string destinationMp3File)
    {
        var tags = new RecordingTags
        {
            Title = string.Format(
                "Recording {0} from {1} to {2}",
                _recordingStarted.ToString("yyyy-MM-dd"),
                _recordingStarted.ToString("HH-mm-ss"),
                _recordingEnded.ToString("HH-mm-ss")),
            Artist = Environment.UserName,
            Album = "Audio recordings",
            Year = _recordingEnded.Year.ToString(),
            Genre = "Audio recording"
        };

        RecordingFileEncoder.WriteMp3(sourceWaveFile, destinationMp3File, tags, _log);
    }

    private void MixFiles(string inputWaveFile1, string inputWaveFile2, string resultWaveFile)
    {
        _log.Debug("Preparing to mix microphone and speaker recordings.");
        using var reader1 = new WaveFileReader(inputWaveFile1);
        using var reader2 = new WaveFileReader(inputWaveFile2);
        Dump(inputWaveFile1, reader1.WaveFormat);
        Dump(inputWaveFile2, reader2.WaveFormat);

        var maxChannels = Math.Max(reader1.WaveFormat.Channels, reader2.WaveFormat.Channels);
        var maxRate = Math.Max(reader1.WaveFormat.SampleRate, reader2.WaveFormat.SampleRate);
        var outputFormat = new WaveFormat(
            maxRate > 48000 ? 48000 : maxRate,
            16,
            maxChannels > 2 ? 2 : maxChannels);
        Dump(resultWaveFile, outputFormat);

        var mixed = new MixingSampleProvider(new[]
        {
            Fit(reader1.ToSampleProvider(), outputFormat),
            Fit(reader2.ToSampleProvider(), outputFormat)
        });
        WaveFileWriter.CreateWaveFile16(resultWaveFile, mixed);
    }

    private static ISampleProvider Fit(ISampleProvider source, WaveFormat output)
    {
        ISampleProvider current = source;
        if (current.WaveFormat.Channels > output.Channels)
            current = new MultiplexingSampleProvider(new[] { current }, output.Channels);
        else if (current.WaveFormat.Channels == 1 && output.Channels == 2)
            current = new MonoToStereoSampleProvider(current);

        if (current.WaveFormat.SampleRate != output.SampleRate)
            current = new WdlResamplingSampleProvider(current, output.SampleRate);

        return current;
    }

    private void Dump(string file, WaveFormat wave)
    {
        _log.Debug($"Wave file '{file}' format: SampleRate {wave.SampleRate}, Channels {wave.Channels}, AverageBytesPerSecond {wave.AverageBytesPerSecond}, Encoding {wave.Encoding}, BitsPerSample {wave.BitsPerSample}");
    }

    private static void StopSession(ref ICaptureSession session)
    {
        if (session == null)
            return;

        try
        {
            session.Stop();
        }
        catch
        {
            // The caller is already failing. Drop the partial capture.
        }

        DeleteSessionFile(ref session);
    }

    private static void DeleteSessionFile(ref ICaptureSession session)
    {
        if (session == null)
            return;

        if (session.WaveFile != null && File.Exists(session.WaveFile))
            File.Delete(session.WaveFile);

        session = null;
    }
}
