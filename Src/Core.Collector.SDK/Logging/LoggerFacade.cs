// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using NLog;
using System;
using System.Diagnostics;

namespace Collector.SDK.Logging
{
    public class LoggerFacade : ILogger
    {
        private readonly AutoFac.Extras.NLog.DotNetCore.ILogger _logger;
        public LoggerFacade(AutoFac.Extras.NLog.DotNetCore.ILogger logger)
        {
            _logger = logger;
        }
        public void Debug(Exception e, string format, params object[] args)
        {
            var logEvent = GetLogEvent(LogLevel.Debug, e, format, args);
            _logger.Log(logEvent);
        }

        public void Error(Exception e, string format, params object[] args)
        {
            var logEvent = GetLogEvent(LogLevel.Error, e, format, args);
            _logger.Log(logEvent);
        }

        public void Fatal(Exception e, string format, params object[] args)
        {
            var logEvent = GetLogEvent(LogLevel.Fatal, e, format, args);
            _logger.Log(logEvent);
        }

        public void Info(Exception e, string format, params object[] args)
        {
            var logEvent = GetLogEvent(LogLevel.Info, e, format, args);
            _logger.Log(logEvent);
        }

        public void Trace(Exception e, string format, params object[] args)
        {
            var logEvent = GetLogEvent(LogLevel.Trace, e, format, args);
            _logger.Log(logEvent);
        }

        public void Warn(Exception e, string format, params object[] args)
        {
            var logEvent = GetLogEvent(LogLevel.Warn, e, format, args);
            _logger.Log(logEvent);
        }

        public void Debug(Exception e)
        {
            this.Debug(e, e.Message);
        }

        public void Error(Exception e)
        {
            this.Error(e, e.Message);
        }

        public void Fatal(Exception e)
        {
            this.Fatal(e, e.Message);
        }

        public void Info(Exception e)
        {
            this.Info(e, e.Message);
        }

        public void Trace(Exception e)
        {
            this.Trace(e, e.Message);
        }

        public void Warn(Exception e)
        {
            this.Warn(e, e.Message);
        }

        private LogEventInfo GetLogEvent(LogLevel level, string format, object[] args)
        {
            StackTrace stackTrace = new StackTrace();

            var method = stackTrace.GetFrame(2).GetMethod();
            var name = method.ReflectedType.Name;
            if (name.IndexOf("<>c__DisplayClass") >= 0) {
                name = method.ReflectedType.FullName;
                var index = name.LastIndexOf(".") + 1;
                var length = name.IndexOf("+<>") - index;
                name = name.Substring(index, length);
            }
            var logEvent = new LogEventInfo(level, name, string.Format(format, args));

            return logEvent;
        }

        private LogEventInfo GetLogEvent(LogLevel level, Exception exception, string format, object[] args)
        {
            StackTrace stackTrace = new StackTrace();

            var method = stackTrace.GetFrame(2).GetMethod();
            var name = method.ReflectedType.Name;
            if (name.IndexOf("<>c__DisplayClass") >= 0)
            {
                name = method.ReflectedType.FullName;
                var index = name.LastIndexOf(".") + 1;
                var length = name.IndexOf("+<>") - index;
                name = name.Substring(index, length);
            }

            string assemblyProp = string.Empty;
            string classProp = string.Empty;
            string methodProp = string.Empty;
            string messageProp = string.Empty;
            string innerMessageProp = string.Empty;

            var logEvent = new LogEventInfo(level, name, string.Format(format, args));

            if (exception != null)
            {
                assemblyProp = exception.Source;
                messageProp = exception.Message;
                if (exception.TargetSite != null)
                {
                    classProp = exception.TargetSite.DeclaringType.FullName;
                    methodProp = exception.TargetSite.Name;
                }
                if (exception.InnerException != null)
                {
                    innerMessageProp = exception.InnerException.Message;
                }
            }

            logEvent.Properties["error-source"] = assemblyProp;
            logEvent.Properties["error-class"] = classProp;
            logEvent.Properties["error-method"] = methodProp;
            logEvent.Properties["error-message"] = messageProp;
            logEvent.Properties["inner-error-message"] = innerMessageProp;

            return logEvent;
        }

        public void Debug(string format, params object[] args)
        {
            var logEvent = GetLogEvent(LogLevel.Debug, format, args);
            _logger.Log(logEvent);
        }

        public void Error(string format, params object[] args)
        {
            var logEvent = GetLogEvent(LogLevel.Error, format, args);
            _logger.Log(logEvent);
        }

        public void Fatal(string format, params object[] args)
        {
            var logEvent = GetLogEvent(LogLevel.Fatal, format, args);
            _logger.Log(logEvent);
        }

        public void Info(string format, params object[] args)
        {
            var logEvent = GetLogEvent(LogLevel.Info, format, args);
            _logger.Log(logEvent);
        }

        public void Trace(string format, params object[] args)
        {
            var logEvent = GetLogEvent(LogLevel.Trace, format, args);
            _logger.Log(logEvent);
        }

        public void Warn(string format, params object[] args)
        {
            var logEvent = GetLogEvent(LogLevel.Warn, format, args);
            _logger.Log(logEvent);
        }
    }
}
