using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ConferenceAudioRecorder.Controller;
using HDE.Platform.Logging;

namespace ConferenceAudioRecorder;

public partial class App : Application
{
    internal static AudioRecorderToolController Controller { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Controller = new AudioRecorderToolController(CreateLog());
        Controller.Initialize();

        Dispatcher.UIThread.UnhandledException += (_, e) =>
        {
            Controller.Services.Log.Error(e.Exception);
        };

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
            desktop.ShutdownRequested += (_, _) => Controller.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static ILog CreateLog()
    {
        var log = new HtmlLog(AppPaths.Logs);
        log.Open();
        return log;
    }
}
