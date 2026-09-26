using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ConferenceAudioRecorder.Legal;

public sealed class DocumentLanguage
{
    public DocumentLanguage(string code, string assetFile, string nativeName, bool rtl = false)
    {
        Code = code;
        AssetFile = assetFile;
        NativeName = nativeName;
        Rtl = rtl;
    }

    public string Code { get; }
    public string AssetFile { get; }
    public string NativeName { get; }
    public bool Rtl { get; }
}

public static class DocumentLanguages
{
    public static IReadOnlyList<DocumentLanguage> All { get; } =
    [
        new("en", "en.md", "English"),
        new("am", "am.md", "አማርኛ"),
        new("ar", "ar.md", "العربية", true),
        new("az", "az.md", "Azərbaycan"),
        new("bn", "bn.md", "বাংলা"),
        new("my", "my.md", "မြန်မာ"),
        new("ceb", "ceb.md", "Cebuano"),
        new("zh-CN", "zh-CN.md", "简体中文"),
        new("zh-HK", "zh-HK.md", "粵語"),
        new("cs", "cs.md", "Čeština"),
        new("nl", "nl.md", "Nederlands"),
        new("tl", "tl.md", "Filipino"),
        new("fr", "fr.md", "Français"),
        new("de", "de.md", "Deutsch"),
        new("el", "el.md", "Ελληνικά"),
        new("gu", "gu.md", "ગુજરાતી"),
        new("ha", "ha.md", "Hausa"),
        new("he", "he.md", "עברית", true),
        new("hi", "hi.md", "हिन्दी"),
        new("hu", "hu.md", "Magyar"),
        new("ig", "ig.md", "Igbo"),
        new("id", "id.md", "Bahasa Indonesia"),
        new("it", "it.md", "Italiano"),
        new("ja", "ja.md", "日本語"),
        new("kn", "kn.md", "ಕನ್ನಡ"),
        new("kk", "kk.md", "Қазақша"),
        new("km", "km.md", "ខ្មែរ"),
        new("ko", "ko.md", "한국어"),
        new("ms", "ms.md", "Bahasa Melayu"),
        new("mr", "mr.md", "मराठी"),
        new("ne", "ne.md", "नेपाली"),
        new("pcm-NG", "pcm-NG.md", "Nigerian Pidgin"),
        new("or", "or.md", "ଓଡ଼ିଆ"),
        new("ps", "ps.md", "پښتو", true),
        new("fa", "fa.md", "فارسی", true),
        new("pl", "pl.md", "Polski"),
        new("pt-BR", "pt-BR.md", "Português (Brasil)"),
        new("pa", "pa.md", "ਪੰਜਾਬੀ"),
        new("ro", "ro.md", "Română"),
        new("ru", "ru.md", "Русский"),
        new("sr", "sr.md", "Српски"),
        new("sd", "sd.md", "سنڌي", true),
        new("si", "si.md", "සිංහල"),
        new("so", "so.md", "Soomaali"),
        new("es", "es.md", "Español"),
        new("sw", "sw.md", "Kiswahili"),
        new("sv", "sv.md", "Svenska"),
        new("ta", "ta.md", "தமிழ்"),
        new("te", "te.md", "తెలుగు"),
        new("th", "th.md", "ไทย"),
        new("tr", "tr.md", "Türkçe"),
        new("uk", "uk.md", "Українська"),
        new("ur", "ur.md", "اردو", true),
        new("uz", "uz.md", "Oʻzbekcha"),
        new("vi", "vi.md", "Tiếng Việt"),
        new("yo", "yo.md", "Yorùbá"),
        new("zu", "zu.md", "isiZulu"),
    ];

    public static DocumentLanguage ByCode(string code) =>
        All.FirstOrDefault(l => string.Equals(l.Code, code, StringComparison.OrdinalIgnoreCase))
        ?? All[0];

    public static DocumentLanguage MatchDevice(CultureInfo culture = null)
    {
        culture ??= CultureInfo.CurrentUICulture;
        var lang = culture.TwoLetterISOLanguageName.ToLowerInvariant();
        var name = culture.Name;

        if (lang == "zh" && (name.Contains("Hant", StringComparison.OrdinalIgnoreCase)
            || name.EndsWith("-HK", StringComparison.OrdinalIgnoreCase)
            || name.EndsWith("-MO", StringComparison.OrdinalIgnoreCase)
            || name.EndsWith("-TW", StringComparison.OrdinalIgnoreCase)
            || name.Contains("yue", StringComparison.OrdinalIgnoreCase)))
            return ByCode("zh-HK");
        if (lang == "zh")
            return ByCode("zh-CN");
        if (lang == "pt")
            return ByCode("pt-BR");
        if (lang is "in" or "id")
            return ByCode("id");
        if (lang is "fil" or "tl")
            return ByCode("tl");
        if (string.Equals(name, "pcm-NG", StringComparison.OrdinalIgnoreCase) || lang == "pcm")
            return ByCode("pcm-NG");

        return All.FirstOrDefault(l =>
                   l.Code.Equals(name, StringComparison.OrdinalIgnoreCase)
                   || l.Code.Equals(lang, StringComparison.OrdinalIgnoreCase))
               ?? ByCode("en");
    }
}
