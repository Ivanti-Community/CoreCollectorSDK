using Collector.SDK.Configuration;
using Collector.SDK.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using System.IO;
using FluentAssertions;
using System;

namespace Collector.SDK.Tests
{
    [TestClass]
    public class LoggerTests
    {
        private string TYPE_LOGGER = "Collector.SDK.Logging.LoggerFacade, Core.Collector.SDK";

        private Logger _nlogger = LogManager.GetCurrentClassLogger();
        private string _path = Directory.GetCurrentDirectory() + @"\CollectorEngine.log";

        [TestInitialize]
        public void Init()
        {
            File.Delete(_path);

            ComponentRegistration.Reset();
            ComponentRegistration.RegisterComponent<Logging.ILogger>(TYPE_LOGGER);
            ComponentRegistration.RegisterModulesFromAssembly(new ThirdPartyAutofacConfiguration()
            {
                AssemblyName = ConfigurationTests.NLOG_EXTRAS_ASSEMBLY,
                RegisterAll = true
            });
            ComponentRegistration.Build();

            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget() { FileName = _path, Name = "logfile" };
            var logconsole = new NLog.Targets.ConsoleTarget() { Name = "logconsole" };
            config.LoggingRules.Add(new NLog.Config.LoggingRule("*", LogLevel.Info, logconsole));
            config.LoggingRules.Add(new NLog.Config.LoggingRule("*", LogLevel.Debug, logfile));
            LogManager.Configuration = config;
        }
        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void LoggerFacade_LogDebugException_Success()
        {
            var logger = ComponentRegistration.CreateInstance<Logging.ILogger>(TYPE_LOGGER);
            logger.Should().NotBeNull();
            logger.Debug(new Exception("Test_1"));

            var content = File.ReadAllText(_path);
            content.Should().Contain("DEBUG");
            content.Should().Contain("Test_1");
        }
        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void LoggerFacade_LogErrorException_Success()
        {
            var logger = ComponentRegistration.CreateInstance<Logging.ILogger>(TYPE_LOGGER);
            logger.Should().NotBeNull();
            logger.Error(new Exception("Test_11"));

            var content = File.ReadAllText(_path);
            content.Should().Contain("ERROR");
            content.Should().Contain("Test_11");
        }
        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void LoggerFacade_LogFatalException_Success()
        {
            var logger = ComponentRegistration.CreateInstance<Logging.ILogger>(TYPE_LOGGER);
            logger.Should().NotBeNull();
            logger.Fatal(new Exception("Test_12"));

            var content = File.ReadAllText(_path);
            content.Should().Contain("FATAL");
            content.Should().Contain("Test_12");
        }
        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void LoggerFacade_LogDebug_Success()
        {
            var logger = ComponentRegistration.CreateInstance<Logging.ILogger>(TYPE_LOGGER);
            logger.Should().NotBeNull();
            logger.Debug("Test_2");

            var content = File.ReadAllText(_path);
            content.Should().Contain("DEBUG");
            content.Should().Contain("Test_2");
        }
        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void LoggerFacade_LogInfo_Success()
        {
            var logger = ComponentRegistration.CreateInstance<Logging.ILogger>(TYPE_LOGGER);
            logger.Should().NotBeNull();
            logger.Info("Test_3");

            var content = File.ReadAllText(_path);
            content.Should().Contain("INFO");
            content.Should().Contain("Test_3");
        }
        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void LoggerFacade_LogWarning_Success()
        {
            var logger = ComponentRegistration.CreateInstance<Logging.ILogger>(TYPE_LOGGER);
            logger.Should().NotBeNull();
            logger.Warn("Test_4");

            var content = File.ReadAllText(_path);
            content.Should().Contain("WARN");
            content.Should().Contain("Test_4");
        }
        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void LoggerFacade_LogError_Success()
        {
            var logger = ComponentRegistration.CreateInstance<Logging.ILogger>(TYPE_LOGGER);
            logger.Should().NotBeNull();
            logger.Error("Test_5");

            var content = File.ReadAllText(_path);
            content.Should().Contain("ERROR");
            content.Should().Contain("Test_5");
        }
        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void LoggerFacade_LogFatal_Success()
        {
            var logger = ComponentRegistration.CreateInstance<Logging.ILogger>(TYPE_LOGGER);
            logger.Should().NotBeNull();
            logger.Fatal("Test_6");

            var content = File.ReadAllText(_path);
            content.Should().Contain("FATAL");
            content.Should().Contain("Test_6");
        }
    }
}
