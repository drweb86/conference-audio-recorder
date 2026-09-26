using ConferenceAudioRecorder.Controller;

namespace ConferenceAudioRecorder.Commands;

internal class OpenPrivatePolicyLinkCommand
{
    public void Execute(AudioRecorderToolController controller)
    {
        ShellOpen.Open("https://github.com/drweb86/conference-audio-recorder/blob/main/Privacy%20Policy.md");
    }
}
