using System;
using HDE.Platform.Logging;

namespace ConferenceAudioRecorder.Services;

internal static class AudioBackend
{
    public static IAudioBackend Create(ILog log)
    {
        if (OperatingSystem.IsWindows())
            return CreateWindows(log);
        if (OperatingSystem.IsLinux())
            return CreateLinux(log);

        throw new PlatformNotSupportedException("Conference Audio Recorder runs on Windows and Linux.");
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static IAudioBackend CreateWindows(ILog log) => new WindowsAudioBackend(log);

    [System.Runtime.Versioning.SupportedOSPlatform("linux")]
    private static IAudioBackend CreateLinux(ILog log) => new LinuxAudioBackend(log);
}
