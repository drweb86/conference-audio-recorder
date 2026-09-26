using ConferenceAudioRecorder.Controller;

namespace ConferenceAudioRecorder.Commands;

internal class ToggleAudioOutputDeviceCommand
{
    public string Execute(AudioRecorderToolController controller)
    {
        if (controller.Model.Settings.AudioOutputDevice == null)
            return controller.Services.Audio.GetDefaultOutputDevice();

        return null;
    }
}
