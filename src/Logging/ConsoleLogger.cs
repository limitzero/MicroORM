using System;

namespace MicroORM.Logging
{
    public class ConsoleLogger : ILogger
    {
        public bool IsDebugEnabled { get; private set; }
        public bool IsInfoEnabled { get; private set; }
        public bool IsErrorEnabled { get; private set; }
        public bool IsWarnEnabled { get; private set; }
        public bool IsFatalEnabled { get; private set; }

        public ConsoleLogger()
        {
            IsDebugEnabled = true;
            IsErrorEnabled = true;
            IsFatalEnabled = true;
            IsWarnEnabled = true;
        }

        public void Debug(string message)
        {
            Log("DEBUG", null, null, message);
        }

        public void Debug(string message, Exception exception)
        {
            Log("DEBUG", exception, null, message);
        }

        public void DebugFormat(string format, params object[] args)
        {
            Log("DEBUG", null, null, format, args);
        }

        public void DebugFormat(Exception exception, string format, params object[] args)
        {
            Log("DEBUG", exception, null, format, args);
        }

        public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            Log("DEBUG", null, formatProvider, format, args);
        }

        public void DebugFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            Log("DEBUG", exception, formatProvider, format, args);
        }

        public void Info(string message)
        {
            Log("INFO", null, null, message);
        }

        public void Info(string message, Exception exception)
        {
            Log("INFO", exception, null, message);
        }

        public void InfoFormat(string format, params object[] args)
        {
            Log("INFO", null, null, format, args);
        }

        public void InfoFormat(Exception exception, string format, params object[] args)
        {
            Log("INFO", exception, null, format, args);
        }

        public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            Log("INFO", null, formatProvider, format, args);
        }

        public void InfoFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            Log("INFO", exception, formatProvider, format, args);
        }

        public void Error(string message)
        {
            Log("ERROR", null, null, message);
        }

        public void Error(string message, Exception exception)
        {
            Log("ERROR", exception, null, message);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            Log("ERROR", null, null, format, args);
        }

        public void ErrorFormat(Exception exception, string format, params object[] args)
        {
            Log("ERROR", exception, null, format, args);
        }

        public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            Log("ERROR", null, formatProvider, format, args);
        }

        public void ErrorFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            Log("ERROR", exception, formatProvider, format, args);
        }

        public void Warn(string message)
        {
            Log("WARN", null, null, message);
        }

        public void Warn(string message, Exception exception)
        {
            Log("WARN", exception, null, message);
        }

        public void WarnFormat(string format, params object[] args)
        {
            Log("WARN", null, null, format, args);
        }

        public void WarnFormat(Exception exception, string format, params object[] args)
        {
            Log("WARN", exception, null, format, args);
        }

        public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            Log("WARN", null, formatProvider, format, args);
        }

        public void WarnFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            Log("WARN", exception, formatProvider, format, args);
        }

        public void Fatal(string message)
        {
            Log("FATAL", null, null, message);
        }

        public void Fatal(string message, Exception exception)
        {
            Log("FATAL", exception, null, message);
        }

        public void FatalFormat(string format, params object[] args)
        {
            Log("FATAL", null, null, format, args);
        }

        public void FatalFormat(Exception exception, string format, params object[] args)
        {
            Log("FATAL", exception, null, format, args);
        }

        public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            Log("FATAL", null, formatProvider, format, args);
        }

        public void FatalFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            Log("FATAL", exception, formatProvider, format, args);
        }

        private void Log(string level, Exception exception, IFormatProvider formatProvider,
            string format,
            params object[] args)
        {
            var message = string.Format("MicroORM - {0} - {1} - {2}", System.DateTime.Now,
                level,
                string.Format(formatProvider, format, args));

            if (exception != null)
                message += string.Format(" - {0}", exception);

            System.Console.WriteLine(message);
        }
    }
}