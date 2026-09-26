using System.Globalization;
using System.Resources;

namespace ConferenceAudioRecorder.Localization;

internal static class Strings
{
    private static readonly ResourceManager Resources = new(
        "ConferenceAudioRecorder.Localization.Strings",
        typeof(Strings).Assembly);

    public static string Get(string name)
    {
        return Resources.GetString(name, CultureInfo.CurrentUICulture) ?? name;
    }
}
