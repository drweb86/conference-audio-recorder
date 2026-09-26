using System;
using System.IO;
using System.Text;

namespace ConferenceAudioRecorder.Legal;

public enum LegalDocumentKind
{
    License,
    Privacy,
    ThirdParty
}

public static class LegalDocuments
{
    public static string Load(LegalDocumentKind kind, string assetFile)
    {
        var prefix = kind switch
        {
            LegalDocumentKind.Privacy => "ConferenceAudioRecorder.Privacy.",
            LegalDocumentKind.ThirdParty => "ConferenceAudioRecorder.Notices.",
            _ => "ConferenceAudioRecorder.License."
        };

        var text = Read(prefix + assetFile);
        if (text != null)
            return text;

        if (!string.Equals(assetFile, "en.md", StringComparison.OrdinalIgnoreCase))
            return Load(kind, "en.md");

        return string.Empty;
    }

    private static string Read(string name)
    {
        var assembly = typeof(LegalDocuments).Assembly;
        using var stream = assembly.GetManifestResourceStream(name);
        if (stream == null)
            return null;

        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
