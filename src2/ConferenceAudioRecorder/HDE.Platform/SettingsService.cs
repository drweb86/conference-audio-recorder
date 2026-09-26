using System;
using System.IO;
using System.Reflection;
using HDE.Platform.Logging;
using HDE.Platform.Serialization;

namespace HDE.Platform.Services
{
    public class SettingsService<TSettings>
        where TSettings : new()
    {
        private const string DefaultFileName = "Settings.xml";
        private ILog _log;
        private string _settingsFile;

        public SettingsService(ILog log, string settingsFolder, string fileName = DefaultFileName)
        {
            Initialize(log, settingsFolder, fileName);
        }

        public SettingsService(ILog log)
        {
#if DEBUG
            Initialize(log,
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location),
                    "Debug"),
                DefaultFileName);
#else
            Initialize(log,
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location)),
                DefaultFileName);
#endif
        }

        private void Initialize(ILog log, string settingsFolder, string settingsFileName)
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log));
            if (!log.IsOpened)
                throw new ArgumentException("Log was not opened", nameof(log));
            if (string.IsNullOrWhiteSpace(settingsFolder))
                throw new ArgumentException("Settings folder was not specified.", nameof(settingsFolder));
            if (string.IsNullOrWhiteSpace(settingsFileName))
                throw new ArgumentException("Settings file name was not specified.", nameof(settingsFileName));

            _log = log;
            _settingsFile = Path.Combine(settingsFolder, settingsFileName);

            _log.Debug($"Settings file: {_settingsFile}.");

            if (!Directory.Exists(settingsFolder))
            {
                _log.Info($"Creating settings folder {settingsFolder}.");
                Directory.CreateDirectory(settingsFolder);
            }
        }

        public TSettings Load()
        {
            _log.Info("Loading settings.");

            if (!File.Exists(_settingsFile))
            {
                _log.Info("Settings are missing. Default settings will be returned.");
                return new TSettings();
            }

            try
            {
                return SerializerHelper.Load<TSettings>(_settingsFile);
            }
            catch (Exception unhandledException)
            {
                _log.Error("Failed to load settings. Default settings will be returned.");
                _log.Error(unhandledException);
                return new TSettings();
            }
        }

        public void Save(TSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            SerializerHelper.Save(settings, _settingsFile);
        }
    }
}
