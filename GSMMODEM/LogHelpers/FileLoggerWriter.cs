using System;
using System.IO;
using System.Text;

namespace Harbour.Utils
{
    internal class FileLoggerWriter
    {
        private readonly string _logDir;

        public FileLoggerWriter()
        {
            string defaultDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _logDir = Path.Combine(defaultDirectory, "Logs");
        }

        public void WriteLine(LogLevel level, string group, string message, Exception exception)
        {
            string fileName = DateTime.Now.ToString("yyyyMMdd");
            if (!string.IsNullOrEmpty(group))
                fileName += "--" + group;
            string log = LogFormatter(level, group, message, exception);
            try
            {
                WriteLog(fileName, log);
            }
            catch (DirectoryNotFoundException)
            {
                CreateLogDir();
                WriteLog(fileName, log);
            }
            catch (Exception)
            {
            }
        }

        private void CreateLogDir()
        {
            if (!Directory.Exists(_logDir))
                Directory.CreateDirectory(_logDir);
        }
        private void WriteLog(string fileName, string log)
        {
            File.AppendAllText(Path.Combine(_logDir, $"{fileName}.txt"), log);
        }
        private string LogFormatter(LogLevel level, string group, string message, Exception exception)
        {
            var logBuilder = new StringBuilder();
            logBuilder.Append($"[{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}] ");
            logBuilder.Append(level.ToString().PadRight(8));
            logBuilder.Append(message);
            if (exception != null)
                logBuilder.Append("\r\n   " + exception.ToString());
            logBuilder.AppendLine();

            return logBuilder.ToString();
        }
    }
}
