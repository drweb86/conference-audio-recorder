using System;
using System.Diagnostics;

namespace ConferenceAudioRecorder;

internal static class ShellOpen
{
    public static void Open(string target)
    {
        if (string.IsNullOrWhiteSpace(target))
            return;

        ProcessStartInfo start;
        if (OperatingSystem.IsWindows())
        {
            start = new ProcessStartInfo
            {
                FileName = target,
                UseShellExecute = true
            };
        }
        else if (OperatingSystem.IsMacOS())
        {
            start = new ProcessStartInfo
            {
                FileName = "open",
                ArgumentList = { target }
            };
        }
        else
        {
            start = new ProcessStartInfo
            {
                FileName = "xdg-open",
                ArgumentList = { target }
            };
        }

        Process.Start(start);
    }
}
