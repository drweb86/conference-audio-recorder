using AudioRecorderV4.Utils;
using HDE.Platform.Logging;
using NAudio.Lame;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.IO;
using System.Linq;

namespace AudioRecorderV4.Services
{
    internal class AudioRecorderService(ILog log)
    {
        private readonly ILog _log = log;
        private DateTime _recordingStarted;
        private DateTime _recordingEnded;
        private WasapiLoopbackRecordingJob _recordSpeakerJob;
        private WaveInRecordingJob _waveInRecordingJob;

        public bool IsAudioRecording 
        { 
            get 
            { 
                return _waveInRecordingJob != null || _recordSpeakerJob != null;
            } 
        }
        public void StartRecording(string inputDeviceFriendlyName, string outputDeviceFriendlyName)
        {
            _log.Debug($"Start recording input {inputDeviceFriendlyName}, output {outputDeviceFriendlyName}");

            if (outputDeviceFriendlyName != null)
            {
                _recordSpeakerJob = new WasapiLoopbackRecordingJob(null, outputDeviceFriendlyName);
            }

            if (inputDeviceFriendlyName != null)
            {
                _waveInRecordingJob = new WaveInRecordingJob(null, inputDeviceFriendlyName);
            }

            _recordingStarted = DateTime.Now;

            _waveInRecordingJob?.Start();
            _recordSpeakerJob?.Start();
        }

        public void StopRecording(string folderName)
        {
            _recordingEnded = DateTime.Now;
            string fileToConvert = null;
            try
            {
                _waveInRecordingJob?.Stop();
                _recordSpeakerJob?.Stop();

                if (_waveInRecordingJob?.WaveFile == null || _recordSpeakerJob?.WaveFile == null)
                {
                    fileToConvert = _waveInRecordingJob?.WaveFile ?? _recordSpeakerJob?.WaveFile;
                }
                else
                {
                    var mixedWaveFile = Path.GetTempFileName();

                    MixFiles(_waveInRecordingJob?.WaveFile, _recordSpeakerJob?.WaveFile, mixedWaveFile);

                    fileToConvert = mixedWaveFile;
                }

                if (!Directory.Exists(folderName))
                    Directory.CreateDirectory(folderName);

                if (_waveInRecordingJob?.WaveFile is not null)
                {
                    var waveInFile = FileNameGenerator.GetOutputMp3FileName(_recordingStarted, _recordingEnded, folderName, " - 🎤");
                    ConvertWaveToMp3(_waveInRecordingJob?.WaveFile, waveInFile, _recordingStarted, _recordingEnded);
                }

                if (_recordSpeakerJob?.WaveFile is not null)
                {
                    var speakerFile = FileNameGenerator.GetOutputMp3FileName(_recordingStarted, _recordingEnded, folderName, " - 🔈");
                    ConvertWaveToMp3(_recordSpeakerJob?.WaveFile, speakerFile, _recordingStarted, _recordingEnded);
                }

                var outputMp3FileName = FileNameGenerator.GetOutputMp3FileName(_recordingStarted, _recordingEnded, folderName);
                ConvertWaveToMp3(fileToConvert, outputMp3FileName, _recordingStarted, _recordingEnded);
                
                
            }
            catch (Exception e)
            {
                _log.Error(e);
                throw;
            }
            finally
            {
                if (_waveInRecordingJob != null)
                {
                    File.Delete(_waveInRecordingJob.WaveFile);
                    _waveInRecordingJob = null;
                }
                if (_recordSpeakerJob != null)
                {
                    File.Delete(_recordSpeakerJob?.WaveFile);
                    _recordSpeakerJob = null;
                }

                if (File.Exists(fileToConvert))
                    File.Delete(fileToConvert);
            }
        }

        private void ConvertWaveToMp3(string sourceWaveFile, string destinationMp3File,
            DateTime startRecording, DateTime endRecording)
        {
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             var tag = new ID3TagData
            {
                Title = string.Format("Recording {0} from {1} to {2}", startRecording.ToString("yyyy-MM-dd"),
                    startRecording.ToString("HH-mm-ss"),
                    endRecording.ToString("HH-mm-ss")),
                Artist = Environment.UserName,
                Album = "Audio recordings",
                Year = _recordingEnded.Year.ToString(),
                Genre = "Audio recording",
            };

            using var reader = new AudioFileReader(sourceWaveFile);
            using var writer = new LameMP3FileWriter(destinationMp3File, reader.WaveFormat, 320, tag);
            reader.CopyTo(writer);
        }

        private void Dump(string file, WaveFormat wave)
        {
            _log.Debug($"Wave file '{file}' format: SampleRate {wave.SampleRate}, Channels {wave.Channels}, ExtraSize {wave.ExtraSize}, AverageBytesPerSecond {wave.AverageBytesPerSecond}, Encoding {wave.Encoding}, BitsPerSample {wave.BitsPerSample}");
        }

        private void MixFiles(string inputWaveFile1, string inputWaveFile2, string resultWaveFile)
        {
            _log.Debug("Preparing to mix files using MediaFoundationResampler (required Vista+, Server with Windows Desktop Experience).");
            using var stream1 = new FileStream(inputWaveFile1, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var stream2 = new FileStream(inputWaveFile2, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader1 = new WaveFileReader(stream1);
            using var reader2 = new WaveFileReader(stream2);
            var waveFormats = new[] { reader1.WaveFormat, reader2.WaveFormat };
            Dump(inputWaveFile1, reader1.WaveFormat);
            Dump(inputWaveFile2, reader2.WaveFormat);

            var maxChannels = waveFormats.Max(formats => formats.Channels);
            var maxRate = waveFormats.Max(formats => formats.SampleRate);

            var maxAllowedRate = 48000;
            var maxAllowedChannels = 2;
            var outputFormat = new WaveFormat(
                maxRate > maxAllowedRate ? maxAllowedRate : maxRate,
                16,
                maxChannels > maxAllowedChannels ? maxAllowedChannels : maxChannels);
            Dump(resultWaveFile, outputFormat);

            var maxQuality = 60;
            try
            {
                using var resampler1 = new MediaFoundationResampler(reader1, outputFormat) { ResamplerQuality = maxQuality };
                using var resampler2 = new MediaFoundationResampler(reader2, outputFormat) { ResamplerQuality = maxQuality };
                var resamplers = new[]
                {
                            resampler1.ToSampleProvider(),
                            resampler2.ToSampleProvider()
                        };

                var mixer = new MixingSampleProvider(resamplers);

                WaveFileWriter.CreateWaveFile16(resultWaveFile, mixer);
            }
            catch (Exception e)
            {
                _log.Error(e);
                throw;
            }
        }
    }
}
