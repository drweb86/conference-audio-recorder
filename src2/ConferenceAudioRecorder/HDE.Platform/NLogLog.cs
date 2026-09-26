using System;
using System.Globalization;
using System.IO;
using System.Text;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace HDE.Platform.Logging
{
    public sealed class NLogLog : LogBase
    {
        private readonly string _logsFolder;
        private Logger _logger;

        public NLogLog(string logsFolder)
        {
            _logsFolder = logsFolder ?? throw new ArgumentNullException(nameof(logsFolder));
            if (!Directory.Exists(_logsFolder))
                Directory.CreateDirectory(_logsFolder);
        }

        protected override void OpenInternal()
        {
            _logFile = NextLogFile();
            var config = new LoggingConfiguration();
            var target = new FileTarget("file")
            {
                FileName = _logFile,
                Layout = "${longdate}|${level:uppercase=true}|${message}${onexception:inner=${newline}${exception:format=tostring}}",
                KeepFileOpen = true,
            Encoding = Encoding.UTF8
        };
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, target);
            LogManager.Configuration = config;
            _logger = LogManager.GetLogger("ConferenceAudioRecorder");
        }

        protected override void WriteInternal(LoggingEvent loggingEvent, string message)
        {
            switch (loggingEvent)
            {
                case LoggingEvent.Error:
                    _logger.Error(message);
                    break;
                case LoggingEvent.Warning:
                    _logger.Warn(message);
                    break;
                case LoggingEvent.Info:
                    _logger.Info(message);
                    break;
                default:
                    _logger.Debug(message);
                    break;
            }
        }

        protected override void CloseInternal()
        {
            LogManager.Shutdown();
            _logger = null;
        }

        private string NextLogFile()
        {
            var stamp = DateTime.Now.ToString("yyyy.MM.dd HH.mm.ss", CultureInfo.InvariantCulture);
            var candidate = Path.Combine(_logsFolder, stamp + ".log");
            var index = 2;
            while (File.Exists(candidate))
            {
                candidate = Path.Combine(_logsFolder, stamp + " " + index.ToString(CultureInfo.InvariantCulture) + ".log");
                index++;
            }

            return candidate;
        }
    }
}
