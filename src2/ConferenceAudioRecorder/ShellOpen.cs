using System.Diagnostics;

namespace ConferenceAudioRecorder;

internal static class ShellOpen
{
    public static void Open(string target)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = target,
            UseShellExecute = true
        });
    }
}
