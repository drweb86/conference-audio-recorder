using System;
using System.IO;
using ConferenceAudioRecorder.Localization;

namespace ConferenceAudioRecorder.Services;

internal static class FileNameGenerator
{
    public static string GetOutputMp3FileName(DateTime startRecording, DateTime endRecording, string folderName, string postfix = null)
    {
        var fileName = string.Format(
            Strings.Get("RecordingFileName"),
            startRecording.ToString("yyyy-MM-dd"),
            startRecording.ToString("HH-mm-ss"),
            endRecording.ToString("HH-mm-ss"));

        return Path.Combine(folderName, $"{fileName}{postfix}.mp3");
    }
}
