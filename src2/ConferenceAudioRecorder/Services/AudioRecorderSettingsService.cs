using ConferenceAudioRecorder.Model;
using HDE.Platform.Logging;
using HDE.Platform.Services;

namespace ConferenceAudioRecorder.Services;

internal class AudioRecorderSettingsService : SettingsService<AudioRecorderSettings>
{
    public AudioRecorderSettingsService(ILog log)
        : base(log, AppPaths.SettingsFolder, "Settings-v1.xml")
    {
    }
}
