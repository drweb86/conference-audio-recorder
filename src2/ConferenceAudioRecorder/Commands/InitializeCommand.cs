using System;
using System.IO;
using System.Linq;
using ConferenceAudioRecorder.Controller;
using ConferenceAudioRecorder.Localization;

namespace ConferenceAudioRecorder.Commands;

internal class InitializeCommand
{
    public void Execute(AudioRecorderToolController controller)
    {
        controller.Services.Log.Debug(
            $"OS {Environment.OSVersion.VersionString}, 64-bit {Environment.Is64BitOperatingSystem}");
        controller.Model.LogsFolder = Path.GetDirectoryName(controller.Services.Log.LogFile);
        controller.Model.InputDevices = controller.Services.Audio.GetInputDevices().ToList();
        controller.Model.OutputDevices = controller.Services.Audio.GetOutputDevices().ToList();
        var defaultOutputDevice = controller.Services.Audio.GetDefaultOutputDevice();
        var defaultInputDevice = controller.Services.Audio.GetDefaultInputDevice();
        controller.Model.Settings = controller.Services.AudioRecorderSettingsService.Load();

        if (controller.Model.Settings.SaveRecordingToFolder == null)
        {
            controller.Model.Settings.AudioInputDevice = defaultInputDevice;
            controller.Model.Settings.AudioOutputDevice = defaultOutputDevice;
        }

        if (controller.Model.Settings.AudioInputDevice != null &&
            !controller.Model.InputDevices.Any(device => device == controller.Model.Settings.AudioInputDevice))
        {
            controller.Model.Settings.AudioInputDevice = defaultInputDevice;
        }

        if (controller.Model.Settings.AudioOutputDevice != null &&
            !controller.Model.OutputDevices.Any(device => device == controller.Model.Settings.AudioOutputDevice))
        {
            controller.Model.Settings.AudioOutputDevice = defaultOutputDevice;
        }

        if (string.IsNullOrWhiteSpace(controller.Model.Settings.Theme))
            controller.Model.Settings.Theme = "Dark";

        if (string.IsNullOrWhiteSpace(controller.Model.Settings.SaveRecordingToFolder))
        {
            var localizedFolder = Strings.Get("ConferenceRecordingsFolder");
            controller.Model.Settings.SaveRecordingToFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                localizedFolder);
        }

        if (!Directory.Exists(controller.Model.Settings.SaveRecordingToFolder))
            Directory.CreateDirectory(controller.Model.Settings.SaveRecordingToFolder);
    }
}
