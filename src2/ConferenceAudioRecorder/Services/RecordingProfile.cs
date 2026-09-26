using ConferenceAudioRecorder.Model;
using NAudio.Wave;

namespace ConferenceAudioRecorder.Services;

internal sealed class RecordingProfile
{
    public const int DefaultSampleRate = 48000;
    public const int DefaultBitRate = 320000;
    public const int Channels = 2;

    public static readonly int[] SampleRates = { 44100, DefaultSampleRate };
    public static readonly int[] BitRates = { 128000, 192000, 256000, DefaultBitRate };

    private RecordingProfile(int sampleRate, int bitRate)
    {
        SampleRate = sampleRate;
        BitRate = bitRate;
        WaveFormat = new WaveFormat(sampleRate, 16, Channels);
    }

    public int SampleRate { get; }
    public int BitRate { get; }
    public WaveFormat WaveFormat { get; }

    public static RecordingProfile FromSettings(AudioRecorderSettings settings)
    {
        var sampleRate = settings != null && settings.RecordingSampleRate == 44100
            ? 44100
            : DefaultSampleRate;
        var bitRate = settings != null && IsKnownBitRate(settings.EncodingBitRate)
            ? settings.EncodingBitRate
            : DefaultBitRate;
        return new RecordingProfile(sampleRate, bitRate);
    }

    public static bool IsKnownBitRate(int bitRate)
    {
        for (var i = 0; i < BitRates.Length; i++)
        {
            if (BitRates[i] == bitRate)
                return true;
        }

        return false;
    }

    // libsndfile maps MP3 compression from 0.0 (highest quality) to 1.0 (lowest).
    public double CompressionLevel => CompressionLevelFor(BitRate);

    public static double CompressionLevelFor(int bitRate)
    {
        if (bitRate >= 320000)
            return 0.0;
        if (bitRate >= 256000)
            return 0.2;
        if (bitRate >= 192000)
            return 0.35;
        return 0.55;
    }
}
