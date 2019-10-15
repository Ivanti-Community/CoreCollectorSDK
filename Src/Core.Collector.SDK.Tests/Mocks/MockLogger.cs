using Collector.SDK.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collector.SDK.Tests.Mocks
{
    public class MockLogger : ILogger
    {
        public void Debug(Exception exception)
        {
        }

        public void Debug(string format, params object[] args)
        {
        }

        public void Debug(Exception exception, string format, params object[] args)
        {
        }

        public void Error(Exception exception)
        {
        }

        public void Error(string format, params object[] args)
        {
        }

        public void Error(Exception exception, string format, params object[] args)
        {
        }

        public void Fatal(Exception exception)
        {
        }

        public void Fatal(string format, params object[] args)
        {
        }

        public void Fatal(Exception exception, string format, params object[] args)
        {
        }

        public void Info(Exception exception)
        {
        }

        public void Info(string format, params object[] args)
        {
        }

        public void Info(Exception exception, string format, params object[] args)
        {
        }

        public void Trace(Exception exception)
        {
        }

        public void Trace(string format, params object[] args)
        {
        }

        public void Trace(Exception exception, string format, params object[] args)
        {
        }

        public void Warn(Exception exception)
        {
        }

        public void Warn(string format, params object[] args)
        {
        }

        public void Warn(Exception exception, string format, params object[] args)
        {
        }
    }
}
