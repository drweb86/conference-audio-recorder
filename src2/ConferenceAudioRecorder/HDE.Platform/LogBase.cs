using System;

namespace HDE.Platform.Logging
{
    public abstract class LogBase : ILog, IDisposable
    {
        protected string _logFile;

        public string LogFile => _logFile;

        public bool IsOpened { get; private set; }
        public bool IsErrorsLogged { get; set; }
        public bool IsWarningsLogged { get; set; }

        protected abstract void OpenInternal();
        protected abstract void CloseInternal();
        protected abstract void WriteInternal(LoggingEvent loggingEvent, string message);

        public void Close()
        {
            if (IsOpened)
            {
                CloseInternal();
                IsOpened = false;
            }
        }

        public void Open()
        {
            if (IsOpened)
            {
                throw new LogException("already opened!");
            }

            OpenInternal();
            IsOpened = true;
        }

        public void Write(LoggingEvent loggingEvent, string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                PreprocessLogEntry(loggingEvent, message);
                WriteInternal(loggingEvent, message);
            }
        }

        public void Debug(string message)
        {
            Write(LoggingEvent.Debug, message);
        }

        public void Info(string message)
        {
            Write(LoggingEvent.Info, message);
        }

        public void Error(Exception unhandledException)
        {
            if (unhandledException != null)
            {
                Error(unhandledException.ToString());
            }
        }

        public void Error(string message)
        {
            Write(LoggingEvent.Error, message);
        }

        public void Warning(string message)
        {
            Write(LoggingEvent.Warning, message);
        }

        public void Dispose()
        {
            if (IsOpened)
            {
                Close();
            }
        }

        private void PreprocessLogEntry(LoggingEvent loggingEvent, string message)
        {
            if (!IsOpened)
            {
                System.Diagnostics.Debug.WriteLine("Not opened");
                System.Diagnostics.Debug.WriteLine(Environment.StackTrace);
                throw new LogException("log is closed");
            }

            if (loggingEvent == LoggingEvent.Error)
            {
                IsErrorsLogged = true;
            }

            if (loggingEvent == LoggingEvent.Warning)
            {
                IsWarningsLogged = true;
            }

            System.Diagnostics.Debug.Write($"[{loggingEvent}] ");
            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}
