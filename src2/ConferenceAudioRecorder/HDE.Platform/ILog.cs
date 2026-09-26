using System;

namespace HDE.Platform.Logging
{
    public interface ILog
    {
        string LogFile { get; }

        bool IsOpened { get; }
        void Open();
        void Close();

        bool IsErrorsLogged { get; set; }
        bool IsWarningsLogged { get; set; }

        void Write(LoggingEvent loggingEvent, string message);

        void Debug(string message);

        void Info(string message);

        void Warning(string message);

        void Error(Exception unhandledException);
        void Error(string message);
    }
}
