// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System.Collections.Generic;
using System.Threading;
using Collector.SDK.Collectors;
using Collector.SDK.Configuration;
using Collector.SDK.Converters;
using Collector.SDK.DataModel;
using Collector.SDK.Mappers;
using Collector.SDK.Publishers;
using Collector.SDK.Readers;
using Collector.SDK.Transformers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Collector.SDK.Tests
{
    [TestClass]
    public class CollectorTests
    {
        public const string TYPE_SERIAL_COLLECTOR = "Collector.SDK.Collectors.SerialCollector, Core.Collector.SDK";
        public const string TYPE_SERIAL_STACK = "Collector.SDK.Collectors.Stack, Core.Collector.SDK";
        public const string TYPE_INFINITE_READER = "Collector.SDK.Tests.Mocks.MockInfiniteReader, Core.Collector.SDK.Tests";

        [TestInitialize]
        public void Init()
        {
            ComponentRegistration.Reset();

            ComponentRegistration.RegisterSingletonComponent<ICollector>(TYPE_SERIAL_COLLECTOR);
            ComponentRegistration.RegisterComponent<IStack>(TYPE_SERIAL_STACK);
            ComponentRegistration.RegisterComponent<IConverter>(ConfigurationTests.TYPE_CONVERTER);
            ComponentRegistration.RegisterComponent<IReader>(ConfigurationTests.TYPE_READER);
            ComponentRegistration.RegisterComponent<IReader>(TYPE_INFINITE_READER);
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
        public void Stack_Kill_Success()
        {
            var config = ConfigurationTests.CreateCollectorConfig("1000");
            config.Type = TYPE_SERIAL_COLLECTOR;
            config.StackType = TYPE_SERIAL_STACK;
            config.Readers[0].Type = TYPE_INFINITE_READER;
            var collector = CollectorFactory.CreateCollector(config) as AbstractCollector;
            collector.Should().NotBeNull();

            var stack = collector.CreateStack(config.Readers[0].Id);
            stack.Run(new Dictionary<string, string>());

            stack.Kill();

            stack.Done.Should().BeTrue();
            stack.Reader.Done.Should().BeTrue();
        }

        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void SerialCollector_Run_Success()
        {
            var config = ConfigurationTests.CreateCollectorConfig("1000");
            config.Type = TYPE_SERIAL_COLLECTOR;
            config.StackType = TYPE_SERIAL_STACK;
            var collector = CollectorFactory.CreateCollector(config) as AbstractCollector;
            collector.Should().NotBeNull();

            collector.Run().Wait();

            collector.GetTotalEventCount().Should().Be(3);
        }
        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void Stack_Run_Success()
        {
            var config = ConfigurationTests.CreateCollectorConfig("1000");
            config.Type = TYPE_SERIAL_COLLECTOR;
            config.StackType = TYPE_SERIAL_STACK;
            var collector = CollectorFactory.CreateCollector(config) as AbstractCollector;
            collector.Should().NotBeNull();

            var stack = collector.CreateStack(config.Readers[0].Id);
            stack.Done.Should().BeFalse();
            stack.Reader.Done.Should().BeFalse();
            foreach (var publisher in stack.Publishers)
            {
                publisher.Done.Should().BeFalse();
            }
            foreach (var transformer in stack.Transformers)
            {
                transformer.Done.Should().BeFalse();
            }

            stack.Run(new Dictionary<string, string>()).Wait();
            stack.Done.Should().BeTrue();
            stack.Reader.Done.Should().BeTrue();
            foreach (var publisher in stack.Publishers)
            {
                publisher.Done.Should().BeTrue();
            }
            foreach (var transformer in stack.Transformers)
            {
                transformer.Done.Should().BeTrue();
            }
        }

    }
}
