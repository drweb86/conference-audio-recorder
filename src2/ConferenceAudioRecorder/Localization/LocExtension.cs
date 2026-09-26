using System;
using Avalonia.Markup.Xaml;

namespace ConferenceAudioRecorder.Localization;

public class LocExtension : MarkupExtension
{
    public LocExtension()
    {
    }

    public LocExtension(string key)
    {
        Key = key;
    }

    public string Key { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return Strings.Get(Key);
    }
}
