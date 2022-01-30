﻿using AudioRecorderV4;
using Microsoft.UI.Xaml;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HDE.AudioRecorder.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ApplicationView
    {
        public ApplicationView()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        private void OnOpenLogsFolder(object sender, RoutedEventArgs e)
        {
            App.Controller.OpenLogsFolder();
        }

        private void OnOpenSupportLink(object sender, RoutedEventArgs e)
        {
            App.Controller.OpenSupportLink();
        }

        private void OnOpenLicenseLink(object sender, RoutedEventArgs e)
        {
            App.Controller.OpenLicenseLink();
        }

        private void OnOpenPrivatePolicyLink(object sender, RoutedEventArgs e)
        {
            App.Controller.OpenPrivatePolicyLink();
        }
    }
}
