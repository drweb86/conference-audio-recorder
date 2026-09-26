using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using ConferenceAudioRecorder.Localization;

namespace ConferenceAudioRecorder;

internal static class MessageDialog
{
    public static Task Show(Control owner, string message, string closeText)
    {
        var window = TopLevel.GetTopLevel(owner) as Window;
        var dialog = new Window
        {
            Width = 460,
            SizeToContent = SizeToContent.Height,
            MinHeight = 160,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Title = Strings.Get("ConferenceAudioRecorder"),
            Content = BuildContent(message, closeText, out var close)
        };

        close.Click += (_, _) => dialog.Close();
        return window == null ? ShowWithoutOwner(dialog) : dialog.ShowDialog(window);
    }

    private static Task ShowWithoutOwner(Window dialog)
    {
        var source = new TaskCompletionSource();
        dialog.Closed += (_, _) => source.TrySetResult();
        dialog.Show();
        return source.Task;
    }

    private static StackPanel BuildContent(string message, string closeText, out Button close)
    {
        close = new Button
        {
            Content = closeText,
            HorizontalAlignment = HorizontalAlignment.Right,
            MinWidth = 96,
            IsDefault = true
        };

        var panel = new StackPanel
        {
            Margin = new Avalonia.Thickness(20),
            Spacing = 16
        };
        panel.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap
        });
        panel.Children.Add(close);
        return panel;
    }
}
