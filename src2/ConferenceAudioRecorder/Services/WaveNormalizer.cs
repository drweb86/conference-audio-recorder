using System;
using System.IO;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace ConferenceAudioRecorder.Services;

internal static class WaveNormalizer
{
    public static void Normalize(string waveFile)
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.Copy(waveFile, tempFile, true);

            float max;
            using (var reader = new WaveFileReader(tempFile))
            {
                max = Peak(reader.ToSampleProvider(), reader.WaveFormat.SampleRate);
            }

            if (max == 0 || max > 1.0f)
                return;

            using var source = new WaveFileReader(tempFile);
            var amplified = new VolumeSampleProvider(source.ToSampleProvider())
            {
                Volume = 1.0f / max
            };
            WaveFileWriter.CreateWaveFile16(waveFile, amplified);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    private static float Peak(ISampleProvider samples, int sampleRate)
    {
        float max = 0;
        var buffer = new float[Math.Max(sampleRate, 1)];
        int read;
        while ((read = samples.Read(buffer)) > 0)
        {
            for (int n = 0; n < read; n++)
            {
                var abs = Math.Abs(buffer[n]);
                if (abs > max)
                    max = abs;
            }
        }

        return max;
    }
}
