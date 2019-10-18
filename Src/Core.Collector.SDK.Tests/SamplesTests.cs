// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using Collector.SDK.Collectors;
using Collector.SDK.Configuration;
using Collector.SDK.Converters;
using Collector.SDK.DataModel;
using Collector.SDK.Mappers;
using Collector.SDK.Publishers;
using Collector.SDK.Samples.Collectors;
using Collector.SDK.Samples.Converters;
using Collector.SDK.Samples.DataModel;
using Collector.SDK.Samples.Readers;
using Collector.SDK.Samples.Transformers.DataModel;
using Collector.SDK.Tests;
using Collector.SDK.Tests.Mocks;
using Collector.SDK.Transformers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Collector.SDK.Samples.Tests
{
    [TestClass]
    public class SamplesTests
    {
        private static readonly string TYPE_FOLDER_MONITOR_COLLECTOR = "Collector.SDK.Samples.Collectors.FolderMonitorCollector, Core.Collector.SDK.Samples";
        private static readonly string TYPE_LOG_PUBLISHER = "Collector.SDK.Samples.Publishers.LogPublisher, Core.Collector.SDK.Samples";
        private static readonly string TYPE_SAMPLE_TRANSFORMER = "Collector.SDK.Samples.Transformers.SampleTransformer, Core.Collector.SDK.Samples";
        private static readonly string TYPE_SAMPLE_DIRECTORY = "Collector.SDK.Samples.Collectors.Folder, Core.Collector.SDK.Samples";

        [TestInitialize]
        public void Init()
        {
            ComponentRegistration.Reset();

            ComponentRegistration.RegisterComponent<IMapper>(ConfigurationTests.TYPE_MAPPER);
            ComponentRegistration.RegisterComponent<IStack>(ConfigurationTests.TYPE_STACK);
            ComponentRegistration.RegisterComponent<IConverter>(ConfigurationTests.TYPE_CONVERTER);
            ComponentRegistration.RegisterComponent<IEntity>(ConfigurationTests.TYPE_DATA);
            ComponentRegistration.RegisterComponent<ITransformedDataHandler>(ConfigurationTests.TYPE_TRANSFORMATION_HANDLER);

            ComponentRegistration.RegisterComponent<ICollector>(TYPE_FOLDER_MONITOR_COLLECTOR);
            ComponentRegistration.RegisterComponent<IPublisher>(TYPE_LOG_PUBLISHER);
            ComponentRegistration.RegisterComponent<ITransformer>(TYPE_SAMPLE_TRANSFORMER);
            ComponentRegistration.RegisterComponent<IDirectory>(TYPE_SAMPLE_DIRECTORY);

            ComponentRegistration.RegisterTypesFromAssembly(new ThirdPartyAutofacConfiguration()
            {
                AssemblyName = ConfigurationTests.COLLECTOR_SDK_ASSEMBLY,
                Type = "LoggerFacade"
            });
            ComponentRegistration.RegisterModulesFromAssembly(new ThirdPartyAutofacConfiguration()
            {
                AssemblyName = ConfigurationTests.NLOG_EXTRAS_ASSEMBLY,
                RegisterAll = true
            });

            ComponentRegistration.Build();
        }

        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void NetworkConverter_Convert_Success()
        {
            var config = new ConverterConfiguration()
            {
                LeftSideMap = new Dictionary<string, List<string>>() { { "MacAddress", new List<string>() { "NetworkCard" } } },
                Properties = new Dictionary<string, string>()
            };
            config.Properties.Add("IP Address", "IPAddress");

            var converter = new NetworkConverter();
            converter.Configure(config);

            var macDataPoint = new KeyValuePair<string, object>("MacAddress", "00:11:22:33:44:55");
            var data = new EntityCollection();
            data.Entities.Add(macDataPoint.Key, macDataPoint.Value);

            var ipDataPoint = new KeyValuePair<string, object>("IPAddress", "1.2.3.4");
            data.Entities.Add(ipDataPoint.Key, ipDataPoint.Value);

            var convertedPoints = converter.Convert(macDataPoint, data);
            convertedPoints.Should().NotBeNull();

            var networkCard = convertedPoints["NetworkCard"] as NetworkCard;
            networkCard.MacAddress.Should().Equals("00:11:22:33:44:55");
            networkCard.IPAddress.Should().Equals("1.2.3.4");
        }

        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void SQLPrinterReader_Success()
        {
            var logger = new MockLogger();
            var collector = new MockCollector(logger);
            var sqlReader = new MockSQLReader();
            var reader = new SQLPrinterReader(sqlReader, logger, collector);
            var handler = new MockHandler();
            var config = new EndPointConfiguration()
            {
                Id = "2",
                Password = "password",
                User = "test",
            };
            config.Properties.Add("ServerName", "localhost");
            config.Properties.Add("Database", "MdmPrinter");
            config.Properties.Add("SqlCommand", "SELECT * FROM PRINTER");
            config.Properties.Add("Top", "100");

            reader.Configure("1", config, handler);
            reader.Run(new Dictionary<string, string>()).Wait();
            collector.GetTotalEventCount().Should().Be(1);
        }

        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void SampleTransformer_Transform_Success()
        {
            var readerId = "2";
            var transformer = ComponentRegistration.CreateInstance<ITransformer>(TYPE_SAMPLE_TRANSFORMER);
            var config = ConfigurationTests.CreateTransformerConfig("4", ConfigurationTests.CreateMapperConfig("3", "4"), readerId);
            var handler = new MockTransformationHandler();
            transformer.Configure(config, handler);

            var dataPoint = new KeyValuePair<string, object>("ABC", "Some Value");

            var dataRow = new EntityCollection();
            dataRow.Entities.Add(dataPoint.Key, dataPoint.Value);

            var data = new List<IEntityCollection>();
            data.Add(dataRow);

            transformer.HandleData(readerId, data, new Dictionary<string, string>()).Wait();

            handler.Invoked.Should().BeTrue();
            handler.SenderId.Should().Equals(readerId);

            handler.Data.Count.Should().Equals(1);
            foreach (var row in handler.Data) {
                row.GetType().Should().Be(typeof(MockEntity));
            }
        }
        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void LogPublisher_Publish_Success()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            //var outFileName = string.Format(CultureInfo.InvariantCulture, "{0}\\publisher-log.txt", currentDirectory);
            var outFileName = string.Format("{0}\\publisher-log-{1}{2}{3}{4}{5}.txt",
                                currentDirectory, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Year, DateTime.Now.Hour, DateTime.Now.Minute);
            

            var publisher = ComponentRegistration.CreateInstance<IPublisher>("Collector.SDK.Samples.Publishers.LogPublisher, Core.Collector.SDK.Samples");
            var config = ConfigurationTests.CreateEndPointConfig("1", CollectorConstants.KEY_FOLDER, currentDirectory);
            publisher.Configure("5", config);

            var entity = new LogEntry()
            {
                DateTime = DateTime.Now,
                DateTimeUTC = DateTime.Now.ToUniversalTime(),
                Type = "INFO",
                Module = "SomeModuleName",
                Message = "Some log message"
            };
            var data = new List<object>();
            data.Add(entity);

            var context = new Dictionary<string, string>();

            publisher.PublishData("3", data, context).Wait();

            File.Exists(outFileName).Should().BeTrue();

            var text = File.ReadAllText(outFileName);
            //text.Should().Contain("\"TYPE\":\"INFO\"");
            text.Should().Contain("\"Module\":\"SomeModuleName\"");
            text.Should().Contain("\"Message\":\"Some log message\"");
        }
    }
}
