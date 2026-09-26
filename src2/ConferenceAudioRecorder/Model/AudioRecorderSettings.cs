namespace ConferenceAudioRecorder.Model;

public class AudioRecorderSettings
{
    public string AudioInputDevice { get; set; }
    public string AudioOutputDevice { get; set; }
    public string SaveRecordingToFolder { get; set; }
    public int RecordingSampleRate { get; set; }
    public int EncodingBitRate { get; set; }
    public string Theme { get; set; }
    public string DocumentLanguage { get; set; }
}
