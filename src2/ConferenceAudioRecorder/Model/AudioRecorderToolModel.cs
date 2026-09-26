using System.Collections.Generic;

namespace ConferenceAudioRecorder.Model;

internal class AudioRecorderToolModel
{
    public List<string> InputDevices { get; internal set; }
    public List<string> OutputDevices { get; internal set; }
    public AudioRecorderSettings Settings { get; set; }
    public string LogsFolder { get; internal set; }
}
