﻿using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Reflection;

namespace AudioRecorderV4
{
    public sealed partial class MainWindow : Window
    {
        private Windows.ApplicationModel.Resources.ResourceLoader _resourceLoader;
        public MainWindow()
        {
            this.InitializeComponent();
            NavigateToView("RecordingView");
            _resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();
            Title = _resourceLoader.GetString("ConferenceAudioRecorder");
            App.Controller.UpdatedAudioDevices += OnUpdatedAudioDevices;
        }

        private void OnUpdatedAudioDevices(object sender, EventArgs e)
        {
            DispatcherQueue.TryEnqueue(() => RefreshView());
        }

        private void RefreshView()
        {
            NavigateToView("RecordingView");
        }

        private NavigationViewItem _lastItem;
        private void NavigationView_OnItemInvoked(
            object sender,
            NavigationViewItemInvokedEventArgs args)
        {
            var item = args.InvokedItemContainer as NavigationViewItem;
            if (item == null || item == _lastItem)
                return;

            var clickedView = "Settings";
            if (item.Name != "SettingsItem")
            {
                clickedView = item.Tag?.ToString();
            }
            if (!NavigateToView(clickedView)) return;
            _lastItem = item;
        }

        private bool NavigateToView(string clickedView)
        {
            var view = Assembly.GetExecutingAssembly()
                .GetType($"HDE.AudioRecorder.Views.{clickedView}");

            if (string.IsNullOrWhiteSpace(clickedView) || view == null)
            {
                return false;
            }

            ContentFrame.Navigate(view, null, new EntranceNavigationTransitionInfo());
            return true;
        }

        private void ContentFrame_OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new NavigationException(
                $"Navigation failed {e.Exception.Message} for {e.SourcePageType.FullName}");
        }

        private void NavView_OnBackRequested(
            object sender,
            NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack)
                ContentFrame.GoBack();
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            App.Controller.UpdatedAudioDevices -= OnUpdatedAudioDevices;
            App.Controller.Dispose();
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            ReduceWindowSize();
        }

        bool _reduced = false;
        private void ReduceWindowSize()
        {
            if (_reduced)
            {
                return;
            }
            // Use 'this' rather than 'window' as variable if this is about the current window.
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 500, Height = 250 });
            _reduced = true;
        }
    }

    internal class NavigationException : Exception
    {
        public NavigationException(string msg) : base(msg)
        {

        }
    }
}
