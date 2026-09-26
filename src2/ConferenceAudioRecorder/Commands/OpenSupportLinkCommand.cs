using ConferenceAudioRecorder.Controller;

namespace ConferenceAudioRecorder.Commands;

internal class OpenSupportLinkCommand
{
    public void Execute(AudioRecorderToolController controller)
    {
        ShellOpen.Open("https://github.com/drweb86/conference-audio-recorder/issues");
    }
}
