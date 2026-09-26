using ConferenceAudioRecorder.Controller;

namespace ConferenceAudioRecorder.Commands;

internal class OpenOutputFolderCommand
{
    public void Execute(AudioRecorderToolController controller)
    {
        ShellOpen.Open(controller.Model.Settings.SaveRecordingToFolder);
    }
}
