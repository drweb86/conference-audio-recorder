using ConferenceAudioRecorder.Controller;

namespace ConferenceAudioRecorder.Commands;

internal class OpenLogsFolderCommand
{
    public void Execute(AudioRecorderToolController controller)
    {
        ShellOpen.Open(controller.Model.LogsFolder);
    }
}
