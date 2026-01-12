using System;

namespace AudioRecorderV4.Services
{
    using NAudio.CoreAudioApi;
    using NAudio.Wave;
    using System.IO;
    using System.Threading;

    public class WasapiLoopbackRecordingJob
    {
        private Thread _recordingThread;
        private readonly string _waveFile;
        private readonly string _renderMulitmediaDeviceFriendlyName;
        private readonly CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _cancellationToken;
        private readonly AutoResetEvent _started = new AutoResetEvent(false);
        private readonly AutoResetEvent _stopped = new AutoResetEvent(false);
        private readonly WaveFormat _waveFormat = new WaveFormat();
        private readonly bool _autoAdjustVolume;

        public string WaveFile => _waveFile;

        /// <summary>
        /// Creates job.
        /// </summary>
        /// <param name="waveFile">The destination file name. Temporary file will be used if null.</param>
        /// <param name="renderMulitmediaDeviceFriendlyName">The render multimedia active device friendly name. If null default output device will be used.</param>
        /// <param name="waveFormat">Recording wave format: bit must be 16, channels must be 1 or 2.</param>
        /// <param name="autoAdjustVolume">Recording from speakers will have low volume. Normalization increases volume to the maximum possible.</param>
        public WasapiLoopbackRecordingJob(string waveFile = null, string renderMulitmediaDeviceFriendlyName = null, WaveFormat waveFormat = null, bool autoAdjustVolume = true)
        {
            _waveFile = waveFile ?? Path.GetTempFileName();
            _renderMulitmediaDeviceFriendlyName = renderMulitmediaDeviceFriendlyName;
            _cancellationToken = _cancelTokenSource.Token;
            _waveFormat = waveFormat ?? new WaveFormat();
            _autoAdjustVolume = autoAdjustVolume;

            if (_waveFormat.Channels != 1 && _waveFormat.Channels != 2)
                throw new NotSupportedException("waveFormat.Channels must be 1 or 2.");
            if (_waveFormat.BitsPerSample != 16)
                throw new NotSupportedException("waveFormat.BitsPerSample must be 16.");
        }

        public void Start()
        {
            EnsureFileNotExists();

            _recordingThread = new Thread(new ThreadStart(RecordInThread));
            _recordingThread.Start();

            _started.WaitOne();
        }

        private MMDevice GetDeviceToRecord(MMDeviceEnumerator mmDeviceEnumerator)
        {
            if (_renderMulitmediaDeviceFriendlyName == null)
            {
                if (mmDeviceEnumerator.HasDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia))
                {
                    return mmDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                }
            }

            foreach (MMDevice device in mmDeviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                if (device.FriendlyName == _renderMulitmediaDeviceFriendlyName)
                    return device;
            }

            foreach (MMDevice device in mmDeviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                return device;
            }

            throw new ArgumentException("Cannot find output active rendering multimedia device!");
        }

        private int GetDeviceNumber(MMDevice device)
        {
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                var capabilities = WaveOut.GetCapabilities(i);
                var comparisonFriendlyName = device.FriendlyName.Length > 31 ? device.FriendlyName.Substring(0, 31) : device.FriendlyName;
                if (capabilities.ProductName == comparisonFriendlyName)
                    return i;
            }
            throw new ArgumentException("Cannot find output device number!");
        }

        private void RecordInThread()
        {
            using (var deviceEnumerator = new MMDeviceEnumerator())
            {
                var outputDevice = GetDeviceToRecord(deviceEnumerator);

                using (var waveOutEvent = new WaveOutEvent())
                {
                    // We're playing silence because WasapiLoopbackCapture will not receive data if Speakers does not play anything.
                    waveOutEvent.DeviceNumber = GetDeviceNumber(outputDevice);
                    waveOutEvent.Init(new SilenceProvider(_waveFormat));
                    waveOutEvent.Play();

                    using (var outputWasapiLoopbackCapture = new WasapiLoopbackCapture(outputDevice))
                    using (var outputFileWriter = new WaveFileWriter(_waveFile, outputWasapiLoopbackCapture.WaveFormat))
                    {
                        int writeCounter = 0;
                        EventHandler<WaveInEventArgs> outputCallback = (object sender, WaveInEventArgs data) =>
                        {
                            outputFileWriter.Write(data.Buffer, 0, data.BytesRecorded);

                            // Flush periodically to prevent I/O buildup in file system buffers
                            if (++writeCounter % 100 == 0)
                            {
                                outputFileWriter.Flush();
                                writeCounter = 0;
                            }
                        };

                        outputWasapiLoopbackCapture.DataAvailable += outputCallback;
                        outputWasapiLoopbackCapture.StartRecording();

                        _started.Set();
                        WaitHandle.WaitAny(new[] { _cancellationToken.WaitHandle });

                        outputWasapiLoopbackCapture.StopRecording();
                        outputWasapiLoopbackCapture.DataAvailable -= outputCallback;
                        outputFileWriter.Flush();
                    }
                    waveOutEvent.Stop();
                }
            }

            if (_autoAdjustVolume)
                Normalize(_waveFile);

            _stopped.Set();
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
