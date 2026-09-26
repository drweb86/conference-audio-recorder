using ConferenceAudioRecorder.Controller;

namespace ConferenceAudioRecorder.Commands;

internal class OpenLicenseLinkCommand
{
    public void Execute(AudioRecorderToolController controller)
    {
        ShellOpen.Open("https://github.com/drweb86/conference-audio-recorder/blob/main/LICENSE");
    }
}
