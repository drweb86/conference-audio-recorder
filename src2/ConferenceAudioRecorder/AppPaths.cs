using System;
using System.IO;

namespace ConferenceAudioRecorder;

internal static class AppPaths
{
    public static string Root => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ConferenceAudioRecorder",
        "1.0");

    public static string Logs => Path.Combine(Root, "Logs");

    public static string SettingsFolder => Path.Combine(Root, "Settings");
}
