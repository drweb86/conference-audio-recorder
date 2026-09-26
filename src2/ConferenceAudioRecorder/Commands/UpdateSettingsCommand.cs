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

    public void UpdateTheme(AudioRecorderToolController controller, string theme)
    {
        if (controller.Model.Settings.Theme != theme)
        {
            controller.Model.Settings.Theme = theme;
            controller.Services.AudioRecorderSettingsService.Save(controller.Model.Settings);
        }
    }

    public void UpdateDocumentLanguage(AudioRecorderToolController controller, string languageCode)
    {
        if (controller.Model.Settings.DocumentLanguage != languageCode)
        {
            controller.Model.Settings.DocumentLanguage = languageCode;
            controller.Services.AudioRecorderSettingsService.Save(controller.Model.Settings);
        }
    }
}
