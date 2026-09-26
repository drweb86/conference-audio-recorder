using System;
using System.Globalization;
using System.IO;
using HDE.Platform.FileIO;

namespace HDE.Platform.Logging
{
    public class HtmlLog : LogBase
    {
        const string TimeFormat = "yyyy.MM.dd HH.mm.ss";

        readonly SyncFile _syncfile = new SyncFile();
        readonly string _logsFolder;
        StreamWriter _output;

        protected string LogHeader { get; set; }
        protected string LogFooter { get; set; }
        protected string LogExtension { get; set; }

        public HtmlLog(string logsFolder, string logHeader)
        {
            LogHeader = logHeader;
            LogExtension = ".html";
            LogFooter = "</body>\r\n</html>";
            _logsFolder = logsFolder ?? throw new ArgumentNullException(nameof(logsFolder));
            if (!Directory.Exists(logsFolder))
            {
                Directory.CreateDirectory(logsFolder);
            }
        }

        public HtmlLog(string logsFolder)
            : this(logsFolder, "<html><head></head><body>")
        {
        }

        protected override void OpenInternal()
        {
            if (!Directory.Exists(_logsFolder))
            {
                Directory.CreateDirectory(_logsFolder);
            }

            try
            {
                do
                {
                    do
                    {
                        _logFile = Path.Combine(_logsFolder,
                            DateTime.Now.ToString(TimeFormat, CultureInfo.CurrentCulture) +
                            LogExtension);
                    }
                    while (File.Exists(_logFile));
                }
                while (!_syncfile.TrySyncFile(_logFile));
            }
            catch (ArgumentException e)
            {
                throw new LogException(e.Message);
            }

            try
            {
                var logStream = new FileStream(_logFile, FileMode.Create, FileAccess.Write, FileShare.Read);
                _output = new StreamWriter(logStream);
                _output.Write(LogHeader);
            }
            catch (Exception e)
            {
                throw new LogException(e.Message, e);
            }
        }

        protected override void WriteInternal(LoggingEvent loggingEvent, string message)
        {
            WriteCore(HtmlLogFormatter.GetHtmlFormattedLogMessage(loggingEvent, message));
        }

        protected override void CloseInternal()
        {
            WriteCore(LogFooter);

            _output.Flush();
            _output.Close();
            _syncfile.Dispose();
        }

        protected void WriteCore(string message)
        {
            _output.WriteLine(message);
            _output.Flush();
        }
    }
}
