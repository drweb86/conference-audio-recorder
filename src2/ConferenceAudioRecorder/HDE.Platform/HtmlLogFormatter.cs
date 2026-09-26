using System;
using System.Globalization;

namespace HDE.Platform.Logging
{
    public static class HtmlLogFormatter
    {
        const string InformationFormat = "{0} {1} - {2}";
        const string InformationFormatWithoutPrefix = "{0} - {1}";
        static readonly string[] LoggingEventsStrings = { "[error]", "[warning]", "[packer]", "[debug]" };
        static bool _includeLoggingEventPrefixes;

        public static bool IncludeLoggingEventPrefixes
        {
            get => _includeLoggingEventPrefixes;
            set => _includeLoggingEventPrefixes = value;
        }

        public static string GetHtmlFormattedLogMessage(LoggingEvent loggingEvent, string message)
        {
            string information = _includeLoggingEventPrefixes
                ? string.Format(CultureInfo.CurrentCulture,
                    InformationFormat,
                    LoggingEventsStrings[(int)loggingEvent],
                    DateTime.Now.ToLongTimeString(),
                    message)
                : string.Format(CultureInfo.CurrentCulture,
                    InformationFormatWithoutPrefix,
                    DateTime.Now.ToLongTimeString(),
                    message);

            string output = "<P STYLE=\"margin-bottom: 0cm\"><FONT FACE=\"Courier New, monospace\" COLOR=\"#";

            switch (loggingEvent)
            {
                case LoggingEvent.Error:
                    output += "ff0000\"><B>" + information + "</B>";
                    break;
                case LoggingEvent.Info:
                    output += "000000\">" + information;
                    break;
                case LoggingEvent.Debug:
                    output += "2323dc\">" + information;
                    break;
                case LoggingEvent.Warning:
                    output += "2300dc\"><B>" + information + "</B>";
                    break;
            }

            return output + "</FONT></P>";
        }
    }
}
