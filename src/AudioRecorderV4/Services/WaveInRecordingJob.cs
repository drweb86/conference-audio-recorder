using System;

namespace AudioRecorderV4.Services
{
    using NAudio.CoreAudioApi;
    using NAudio.Wave;
    using System.IO;
    using System.Threading;

    public class WaveInRecordingJob
    {
        private Thread _recordingThread;
        private readonly string _waveFile;
        private readonly string _inputDeviceFriendlyName;
        private readonly CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _cancellationToken;
        private readonly AutoResetEvent _started = new AutoResetEvent(false);
        private readonly AutoResetEvent _stopped = new AutoResetEvent(false);
        private readonly bool _autoAdjustVolume;

        public string WaveFile => _waveFile;

        public WaveInRecordingJob(string waveFile, string inputDeviceFriendlyName, bool autoAdjustVolume = true)
        {
            _waveFile = waveFile ?? Path.GetTempFileName();
            _inputDeviceFriendlyName = inputDeviceFriendlyName;
            _cancellationToken = _cancelTokenSource.Token;
            _autoAdjustVolume = autoAdjustVolume;
        }

        public void Start()
        {
            EnsureFileNotExists();

            _recordingThread = new Thread(new ThreadStart(RecordInThread));
            _recordingThread.Start();

            _started.WaitOne();
        }

        private void RecordInThread()
        {
            using (var deviceEnumerator = new MMDeviceEnumerator())
            using (var waveSource = new WaveInEvent())
            {
                // Configure buffer settings to prevent buffer overruns during long recordings
                waveSource.BufferMilliseconds = 50;  // Reduce latency from default 100ms
                waveSource.NumberOfBuffers = 4;      // Increase from default 3 for more headroom
                waveSource.DeviceNumber = FindWaveInDeviceName(_inputDeviceFriendlyName);

                using (var inputFileWriter = new WaveFileWriter(_waveFile, waveSource.WaveFormat))
                {
                    int writeCounter = 0;
                    EventHandler<WaveInEventArgs> outputCallback = (object sender, WaveInEventArgs data) =>
                    {
                        inputFileWriter.Write(data.Buffer, 0, data.BytesRecorded);

                        // Flush periodically to prevent I/O buildup in file system buffers
                        if (++writeCounter % 100 == 0)
                        {
                            inputFileWriter.Flush();
                            writeCounter = 0;
                        }
                    };

                    waveSource.DataAvailable += outputCallback;

                    waveSource.StartRecording();

                    _started.Set();
                    WaitHandle.WaitAny(new[] { _cancellationToken.WaitHandle });

                    waveSource.StopRecording();
                    waveSource.DataAvailable -= outputCallback;
                    inputFileWriter.Flush();
                }
            }

            if (_autoAdjustVolume)
                Normalize(_waveFile);

            _stopped.Set();
        }

        private int FindWaveInDeviceName(string inputDeviceFriendlyName)
        {
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                var capabilities = WaveIn.GetCapabilities(i);
                var comparisonFriendlyName = inputDeviceFriendlyName.Length > 31 ? inputDeviceFriendlyName.Substring(0, 31) : inputDeviceFriendlyName;
                if (capabilities.ProductName == comparisonFriendlyName)
                {
                    return i;
                }
            }

            throw new ArgumentException("Cannot find input device number!");
        }

        private void EnsureFileNotExists()
        {
            var directory = Path.GetDirectoryName(_waveFile);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            if (File.Exists(_waveFile))
                File.Delete(_waveFile);
        }

        public void Stop()
        {
            _cancelTokenSource.Cancel();

            _stopped.WaitOne();
        }

        private void Normalize(string waveFile)
        {
            var tempFile = Path.GetTempFileName();

            try
            {
                File.Copy(waveFile, tempFile, true);

                float max = 0;

                using (var reader = new AudioFileReader(tempFile))
                {
                    // find the max peak
                    float[] buffer = new float[reader.WaveFormat.SampleRate];
                    int read;
                    do
                    {
                        read = reader.Read(buffer, 0, buffer.Length);
                        for (int n = 0; n < read; n++)
                        {
                            var abs = Math.Abs(buffer[n]);
                            if (abs > max) max = abs;
                        }
                    } while (read > 0);

                    if (max == 0)
                        return; // Silence. No need to normalize.

                    if (max > 1.0f)
                        return; // File cannot be normalized because it contains invalid data;

                    // rewind and amplify
                    reader.Position = 0;
                    reader.Volume = 1.0f / max;

                    // write out to a new WAV file
                    WaveFileWriter.CreateWaveFile16(waveFile, reader);
                }
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
