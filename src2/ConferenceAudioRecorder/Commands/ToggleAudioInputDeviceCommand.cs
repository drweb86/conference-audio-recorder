using ConferenceAudioRecorder.Controller;

namespace ConferenceAudioRecorder.Commands;

internal class ToggleAudioInputDeviceCommand
{
    public string Execute(AudioRecorderToolController controller)
    {
        if (controller.Model.Settings.AudioInputDevice == null)
            return controller.Services.Audio.GetDefaultInputDevice();

        return null;
    }
}
