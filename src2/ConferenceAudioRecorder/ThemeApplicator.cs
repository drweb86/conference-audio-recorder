using Avalonia;
using Avalonia.Styling;

namespace ConferenceAudioRecorder;

internal static class ThemeApplicator
{
    public const string Dark = "Dark";
    public const string Light = "Light";
    public const string FollowSystem = "System";

    public static void Apply(string theme)
    {
        if (Application.Current == null)
            return;

        Application.Current.RequestedThemeVariant = ToVariant(theme);
    }

    public static ThemeVariant ToVariant(string theme)
    {
        if (string.Equals(theme, Light, System.StringComparison.OrdinalIgnoreCase))
            return ThemeVariant.Light;
        if (string.Equals(theme, FollowSystem, System.StringComparison.OrdinalIgnoreCase))
            return ThemeVariant.Default;
        return ThemeVariant.Dark;
    }
}
