using System;

namespace MicroORM.Logging
{
    public interface ILogger
    {
        bool IsDebugEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsErrorEnabled { get; }
        bool IsWarnEnabled { get; }
        bool IsFatalEnabled { get; }

        /// <summary>
        ///   Logs a debug message.
        /// </summary>
        void Debug(string message);

        /// <summary>
        ///   Logs a debug message.
        /// </summary>
        /// <param name="exception"> The exception to log </param>
        /// <param name="message"> The message to log </param>
        void Debug(string message, Exception exception);

        /// <summary>
        ///   Logs a debug message.
        /// </summary>
        /// <param name="format"> Format string for the message to log </param>
        /// <param name="args"> Format arguments for the message to log </param>
        void DebugFormat(string format, params object[] args);

        /// <summary>
        ///   Logs a debug message.
        /// </summary>
        /// <param name="exception"> The exception to log </param>
        /// <param name="format"> Format string for the message to log </param>
        /// <param name="args"> Format arguments for the message to log </param>
        void DebugFormat(Exception exception, string format, params object[] args);

        /// <summary>
        ///   Logs a debug message.
        /// </summary>
        /// <param name="formatProvider"> The format provider to use </param>
        /// <param name="format"> Format string for the message to log </param>
        /// <param name="args"> Format arguments for the message to log </param>
        void DebugFormat(IFormatProvider formatProvider, string format, params object[] args);

        /// <summary>
        ///   Logs a debug message.
        /// </summary>
        /// <param name="exception"> The exception to log </param>
        /// <param name="formatProvider"> The format provider to use </param>
        /// <param name="format"> Format string for the message to log </param>
        /// <param name="args"> Format arguments for the message to log </param>
        void DebugFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args);

        /// <summary>
        ///   Logs a info message.
        /// </summary>
        void Info(string message);

        /// <summary>
        ///   Logs a info message.
        /// </summary>
        /// <param name="exception"> The exception to log </param>
        /// <param name="message"> The message to log </param>
        void Info(string message, Exception exception);

        /// <summary>
        ///   Logs a info message.
        /// </summary>
        /// <param name="format"> Format string for the message to log </param>
        /// <param name="args"> Format arguments for the message to log </param>
        void InfoFormat(string format, params object[] args);

        /// <summary>
        ///   Logs a info message.
        /// </summary>
        /// <param name="exception"> The exception to log </param>
        /// <param name="format"> Format string for the message to log </param>
        /// <param name="args"> Format arguments for the message to log </param>
        void InfoFormat(Exception exception, string format, params object[] args);

        /// <summary>
        ///   Logs a info message.
        /// </summary>
        /// <param name="formatProvider"> The format provider to use </param>
        /// <param name="format"> Format string for the message to log </param>
        /// <param name="args"> Format arguments for the message to log </param>
        void InfoFormat(IFormatProvider formatProvider, string format, params object[] args);

        /// <summary>
        ///   Logs a info message.
        /// </summary>
        /// <param name="exception"> The exception to log </param>
        /// <param name="formatProvider"> The format provider to use </param>
        /// <param name="format"> Format string for the message to log </param>
        /// <param name="args"> Format arguments for the message to log </param>
        void InfoFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args);


        /// <summary>
        ///   Logs a error message.
        /// </summary>
        void Error(string message);

        /// <summary>
        ///   Logs a error message.
        /// </summary>
        /// <param name="exception"> The exception to log </param>
        /// <param name="message"> The message to log </param>
        void Error(string message, Exception exception);

        /// <summary>
        ///   Logs a error message.
        /// </summary>
        /// <param name="format"> Format string for the message to log </param>
        /// <param name="args"> Format arguments for the message to log </param>
        void ErrorFormat(string format, params object[] args);

        /// <summary>
        ///   Logs a error message.
        /// </summary>
        /// <param name="exception"> The exception to log </param>
        /// <param name="format"> Format string for the message to log </param>
        /// <param name="args"> Format arguments for the message to log </param>
        void ErrorFormat(Exception exception, string format, params object[] args);

        /// <summary>
        ///   Logs a error message.
        /// </summary>
        /// <param name="formatProvider"> The format provider to use </param>
        /// <param name="format"> Format string for the message to log </param>
        /// <param name="args"> Format arguments for the message to log </param>
        void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args);

        /// <summary>
        ///   Logs a error message.
        /// </summary>
        /// <param name="exception"> The exception to log </param>
        /// <param name="formatProvider"> The format provider to use </param>
        /// <param name="format"> Format string for the message to log </param>
        /// <param name="args"> Format arguments for the message to log </param>
        void ErrorFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args);

        /// <summary>
        ///   Logs a warning message.
        /// </summary>
        void Warn(string message);

        /// <summary>
        ///   Logs a warning message.
        /// </summary>
        /// <param name="exception"> The exception to log </param>
        /// <param name="message"> The message to log </param>
        void Warn(string message, Exception exception);

        /// <summary>
        ///   Logs a warning message.
        /// </summary>
        /// <param name="format"> Format string for the message to log </param>
        /// <param name="args"> Format arguments for the message to log </param>
        void WarnFormat(string format, params object[] args);

        /// <summary>
        ///   Logs a warning message.
        /// </summary>
        /// <param name="exception"> The exception to log </param>
        /// <param name="format"> Format string for the message to log </param>
        /// <param name="args"> Format arguments for the message to log </param>
        void WarnFormat(Exception exception, string format, params object[] args);

        /// <summary>
        ///   Logs a warning message.
        /// </summary>
        /// <param name="formatProvider"> The format provider to use </param>
        /// <param name="format"> Format string for the message to log </param>
        /// <param name="args"> Format arguments for the message to log </param>
        void WarnFormat(IFormatProvider formatProvider, string format, params object[] args);

        /// <summary>
        ///   Logs a warning message.
        /// </summary>
        /// <param name="exception"> The exception to log </param>
        /// <param name="formatProvider"> The format provider to use </param>
        /// <param name="format"> Format string for the message to log </param>
        /// <param name="args"> Format arguments for the message to log </param>
        void WarnFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args);

        /// <summary>
        ///   Logs a fatal message.
        /// </summary>
        void Fatal(string message);

        /// <summary>
        ///   Logs a fatal message.
        /// </summary>
        /// <param name="exception"> The exception to log </param>
        /// <param name="message"> The message to log </param>
        void Fatal(string message, Exception exception);

        /// <summary>
        ///   Logs a fatal message.
        /// </summary>
        /// <param name="format"> Format string for the message to log </param>
        /// <param name="args"> Format arguments for the message to log </param>
        void FatalFormat(string format, params object[] args);

        /// <summary>
        ///   Logs a fatal message.
        /// </summary>
        /// <param name="exception"> The exception to log </param>
        /// <param name="format"> Format string for the message to log </param>
        /// <param name="args"> Format arguments for the message to log </param>
        void FatalFormat(Exception exception, string format, params object[] args);

        /// <summary>
        ///   Logs a fatal message.
        /// </summary>
        /// <param name="formatProvider"> The format provider to use </param>
        /// <param name="format"> Format string for the message to log </param>
        /// <param name="args"> Format arguments for the message to log </param>
        void FatalFormat(IFormatProvider formatProvider, string format, params object[] args);

        /// <summary>
        ///   Logs a fatal message.
        /// </summary>
        /// <param name="exception"> The exception to log </param>
        /// <param name="formatProvider"> The format provider to use </param>
        /// <param name="format"> Format string for the message to log </param>
        /// <param name="args"> Format arguments for the message to log </param>
        void FatalFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args);
    }
}