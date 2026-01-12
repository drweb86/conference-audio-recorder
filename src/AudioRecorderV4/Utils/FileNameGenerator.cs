using System;
using System.IO;

namespace AudioRecorderV4.Utils
{
    internal static class FileNameGenerator
    {
        public static string GetOutputMp3FileName(DateTime startRecording, DateTime endRecording, string folderName, string postfix = null)
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();
            var fileFormatString = resourceLoader.GetString("RecordingFileName");
            var fileName = string.Format(fileFormatString,
                startRecording.ToString("yyyy-MM-dd"),
                startRecording.ToString("HH-mm-ss"),
                endRecording.ToString("HH-mm-ss"));

            return Path.Combine(folderName, $"{fileName}{postfix}.mp3");
        }
    }
}
