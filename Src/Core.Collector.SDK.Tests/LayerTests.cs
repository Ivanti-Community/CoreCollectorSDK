using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Collector.SDK.Collectors;
using Collector.SDK.Configuration;
using Collector.SDK.Converters;
using Collector.SDK.DataModel;
using Collector.SDK.Logging;
using Collector.SDK.Mappers;
using Collector.SDK.Publishers;
using Collector.SDK.Readers;
using Collector.SDK.Tests.Mocks;
using Collector.SDK.Transformers;
using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Collector.SDK.Tests
{
    [TestClass]
    public class LayerTests
    {
        [TestInitialize]
        public void Init()
        {
            ComponentRegistration.Reset();

            ComponentRegistration.RegisterSingletonComponent<ICollector>(ConfigurationTests.TYPE_COLLECTOR);
            ComponentRegistration.RegisterComponent<IStack>(ConfigurationTests.TYPE_STACK);
            ComponentRegistration.RegisterComponent<IConverter>(ConfigurationTests.TYPE_CONVERTER);
            ComponentRegistration.RegisterComponent<IReader>(ConfigurationTests.TYPE_READER);
            ComponentRegistration.RegisterComponent<ITransformer>(ConfigurationTests.TYPE_TRANSFORMER);
            ComponentRegistration.RegisterComponent<IPublisher>(ConfigurationTests.TYPE_PUBLISHER);
            ComponentRegistration.RegisterComponent<IMapper>(ConfigurationTests.TYPE_MAPPER);
            ComponentRegistration.RegisterComponent<IEntity>(ConfigurationTests.TYPE_DATA);

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
        public void Reader_Run_TransformerInvoked()
        {
            var config = ConfigurationTests.CreateCollectorConfig("1000");
            var collector = CollectorFactory.CreateCollector(config);

            var endpointConfig = ConfigurationTests.CreateEndPointConfig("1", "FileName", "c://temp//temp.log");
            var readerConfig = ConfigurationTests.CreateReaderConfig("2", "1");
            var handler = new MockHandler();
            var reader = CollectorFactory.CreateReader(readerConfig.Type, readerConfig.Id, endpointConfig, handler) as MockReader;
            reader.Should().NotBeNull();

            reader.Run(new Dictionary<string, string>()).Wait();
 
            handler.Invoked.Should().BeTrue();
        }
        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void Transformer_HandleData_PublisherInvoked()
        {
            var mapperConfig = ConfigurationTests.CreateMapperConfig("3", "4");
            var tconfig = ConfigurationTests.CreateTransformerConfig("4", mapperConfig, "2");
            var handler = new MockTransformationHandler();

            var transformer = CollectorFactory.CreateTransformer(tconfig, handler);
            transformer.Should().NotBeNull();

            List<IEntityCollection> data = new List<IEntityCollection>();
            IEntityCollection row = new EntityCollection();
            row.Entities.Add("foo", "bar");
            data.Add(row);

            transformer.HandleData("2", data, new Dictionary<string, string>()).Wait();

            handler.Invoked.Should().BeTrue();
            handler.Data.Count.Should().BeGreaterThan(0);
        }
        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void Collector_Run_Success()
        {
            var config = ConfigurationTests.CreateCollectorConfig("1000");
            var collector = CollectorFactory.CreateCollector(config);
            collector.Should().NotBeNull();

            collector.Run().Wait();
 
            (collector as MockCollector).GetTotalEventCount().Should().Be(3);
        }
    }
}
