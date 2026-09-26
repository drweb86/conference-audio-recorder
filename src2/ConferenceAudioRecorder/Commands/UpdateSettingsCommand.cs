using ConferenceAudioRecorder.Controller;

namespace ConferenceAudioRecorder.Commands;

internal class UpdateSettingsCommand
{
    public void UpdateAudioInputDevice(AudioRecorderToolController controller, string audioInputDevice)
    {
        if (controller.Model.Settings.AudioInputDevice != audioInputDevice)
        {
            controller.Model.Settings.AudioInputDevice = audioInputDevice;
            controller.Services.AudioRecorderSettingsService.Save(controller.Model.Settings);
        }
    }

    public void UpdateAudioOutputDevice(AudioRecorderToolController controller, string audioOutputDevice)
    {
        if (controller.Model.Settings.AudioOutputDevice != audioOutputDevice)
        {
            controller.Model.Settings.AudioOutputDevice = audioOutputDevice;
            controller.Services.AudioRecorderSettingsService.Save(controller.Model.Settings);
        }
    }

    public void UpdateSaveRecordingToFolder(AudioRecorderToolController controller, string saveRecordingToFolder)
    {
        if (controller.Model.Settings.SaveRecordingToFolder != saveRecordingToFolder)
        {
            controller.Model.Settings.SaveRecordingToFolder = saveRecordingToFolder;
            controller.Services.AudioRecorderSettingsService.Save(controller.Model.Settings);
        }
    }
}
