using System;
using System.IO;
using System.Runtime.Versioning;
using HDE.Platform.Logging;
using NAudio.SoundFile;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace ConferenceAudioRecorder.Services;

internal readonly struct RecordingTags
{
    public string Title { get; init; }
    public string Artist { get; init; }
    public string Album { get; init; }
    public string Year { get; init; }
    public string Genre { get; init; }
}

internal static class RecordingFileEncoder
{
    public static void WriteMp3(string sourceWave, string destinationMp3, RecordingTags tags, ILog log)
    {
        if (TryWriteWithSoundFile(sourceWave, destinationMp3, tags, log))
            return;

        if (OperatingSystem.IsWindows())
        {
            WriteWithMediaFoundation(sourceWave, destinationMp3);
            return;
        }

        throw new InvalidOperationException(
            "Cannot write an MP3 file. Install libsndfile with MP3 support (package libsndfile1 on Debian and Ubuntu).");
    }

    private static bool TryWriteWithSoundFile(string sourceWave, string destinationMp3, RecordingTags tags, ILog log)
    {
        try
        {
            if (!SoundFileCapabilities.IsFormatSupported(SoundFileMajorFormat.Mp3, SoundFileSubtype.Mp3))
            {
                log.Debug("The installed libsndfile cannot write MP3.");
                return false;
            }
        }
        catch (Exception ex)
        {
            log.Debug($"libsndfile is not available for MP3 encoding: {ex.Message}");
            return false;
        }

        using var reader = new WaveFileReader(sourceWave);
        IWaveProvider source = reader.WaveFormat.Encoding is WaveFormatEncoding.Pcm or WaveFormatEncoding.IeeeFloat
            ? reader
            : new SampleToWaveProvider16(reader.ToSampleProvider());

        var options = new SoundFileWriterOptions
        {
            VbrQuality = 0.9,
            Tags = new SoundFileTags
            {
                Title = tags.Title,
                Artist = tags.Artist,
                Album = tags.Album,
                Date = tags.Year,
                Genre = tags.Genre
            }
        };

        SoundFileWriter.CreateSoundFile(destinationMp3, source, SoundFileMajorFormat.Mp3, options);
        return true;
    }

    [SupportedOSPlatform("windows")]
    private static void WriteWithMediaFoundation(string sourceWave, string destinationMp3)
    {
        using var reader = new WaveFileReader(sourceWave);
        MediaFoundationEncoder.EncodeToMp3(reader, destinationMp3, 320000);
    }
}
