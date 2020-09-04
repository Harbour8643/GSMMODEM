using System;

namespace Harbour.Utils
{
    internal partial class FileLogger
    {
        private readonly FileLoggerWriter _fileLoggerWriter = new FileLoggerWriter();
        private readonly string _name;

        public Func<string, LogLevel, bool> Filter { get; set; } = (category, logLevel) => true;

        public FileLogger(string name = null)
        {
            _name = string.IsNullOrEmpty(name) ? nameof(FileLogger) : name;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return Filter(_name, logLevel);
        }

        public void Log<TState>(LogLevel logLevel, string group, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            string message = formatter(state, exception);

            if (string.IsNullOrEmpty(message) && exception == null)
                return;

            _fileLoggerWriter.WriteLine(logLevel, group, message, exception);
        }
    }
}
