using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using ConferenceAudioRecorder.Legal;
using ConferenceAudioRecorder.Localization;

namespace ConferenceAudioRecorder.Views;

public partial class LegalDocumentView : UserControl
{
    private readonly LegalDocumentKind _kind;
    private bool _ready;

    public LegalDocumentView()
        : this(LegalDocumentKind.License)
    {
    }

    public LegalDocumentView(LegalDocumentKind kind)
    {
        _kind = kind;
        InitializeComponent();
        TitleBlock.Text = TitleFor(kind);
        LanguageBox.ItemsSource = DocumentLanguages.All;

        var saved = App.Controller.Model.Settings.DocumentLanguage;
        LanguageBox.SelectedItem = string.IsNullOrWhiteSpace(saved)
            ? DocumentLanguages.MatchDevice()
            : DocumentLanguages.ByCode(saved);
        _ready = true;
        Render();
    }

    private void OnLanguageChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_ready || LanguageBox.SelectedItem is not DocumentLanguage language)
            return;

        App.Controller.UpdateDocumentLanguage(language.Code);
        Render();
    }

    private void OnClose(object sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is MainWindow window)
            window.ShowAbout();
    }

    private void Render()
    {
        var language = LanguageBox.SelectedItem as DocumentLanguage ?? DocumentLanguages.ByCode("en");
        FlowDirection = language.Rtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        DocumentHost.Children.Clear();

        var markdown = LegalDocuments.Load(_kind, language.AssetFile);
        foreach (var block in MarkdownParser.Parse(markdown))
        {
            switch (block)
            {
                case MarkdownBlock.Heading heading:
                    DocumentHost.Children.Add(new TextBlock
                    {
                        Text = heading.Text,
                        FontWeight = FontWeight.Bold,
                        FontSize = heading.Level == 1 ? 22 : heading.Level == 2 ? 18 : 16,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, heading.Level == 1 ? 0 : 12, 0, 8)
                    });
                    break;
                case MarkdownBlock.Paragraph paragraph:
                    DocumentHost.Children.Add(CreateRichText(paragraph.Inlines, 0, 10, null));
                    break;
                case MarkdownBlock.Bullet bullet:
                    DocumentHost.Children.Add(CreateRichText(bullet.Inlines, 12, 6, "•  "));
                    break;
            }
        }
    }

    private static Control CreateRichText(
        System.Collections.Generic.IReadOnlyList<MarkdownInline> inlines,
        double left,
        double bottom,
        string prefix)
    {
        var block = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(left, 0, 0, bottom),
            FontSize = 14,
            Inlines = []
        };
        if (!string.IsNullOrEmpty(prefix))
            block.Inlines.Add(new Run { Text = prefix });

        foreach (var inline in inlines)
        {
            switch (inline)
            {
                case MarkdownInline.Text text:
                    block.Inlines.Add(new Run
                    {
                        Text = text.Value,
                        FontWeight = text.Bold ? FontWeight.Bold : FontWeight.Normal
                    });
                    break;
                case MarkdownInline.Code code:
                    block.Inlines.Add(new Run { Text = code.Value });
                    break;
                case MarkdownInline.Link link when Uri.TryCreate(link.Url, UriKind.Absolute, out var uri):
                    block.Inlines.Add(new InlineUIContainer
                    {
                        Child = new HyperlinkButton
                        {
                            Content = link.Label,
                            NavigateUri = uri,
                            Padding = new Thickness(0),
                            FontSize = 14,
                            VerticalAlignment = VerticalAlignment.Center
                        }
                    });
                    break;
                case MarkdownInline.Link link:
                    block.Inlines.Add(new Run { Text = link.Label });
                    break;
            }
        }

        return block;
    }

    private static string TitleFor(LegalDocumentKind kind)
    {
        return kind switch
        {
            LegalDocumentKind.Privacy => Strings.Get("PrivatePolicyLink"),
            LegalDocumentKind.ThirdParty => Strings.Get("ThirdPartyNotices"),
            _ => Strings.Get("LicenseLink")
        };
    }
}
