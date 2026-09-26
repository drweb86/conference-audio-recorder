using ConferenceAudioRecorder.Services;
using HDE.Platform.Logging;

namespace ConferenceAudioRecorder.Controller;

internal class ServiceContainer : System.IDisposable
{
    public ServiceContainer(ILog log)
    {
        Log = log;
        Audio = AudioBackend.Create(log);
        AudioRecorderService = new AudioRecorderService(log, Audio);
        AudioRecorderSettingsService = new AudioRecorderSettingsService(log);
    }

    public readonly ILog Log;
    public readonly IAudioBackend Audio;
    public readonly AudioRecorderService AudioRecorderService;
    public readonly AudioRecorderSettingsService AudioRecorderSettingsService;

    public void Dispose()
    {
        Audio.Dispose();
        Log.Close();
    }
}
