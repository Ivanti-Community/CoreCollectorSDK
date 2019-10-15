// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.Collectors;
using Collector.SDK.Configuration;
using Collector.SDK.Converters;
using Collector.SDK.DataModel;
using Collector.SDK.Mappers;
using Collector.SDK.Publishers;
using Collector.SDK.Readers;
using Collector.SDK.Tests.Mocks;
using Collector.SDK.Transformers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Autofac.Core;

namespace Collector.SDK.Tests
{
    [TestClass]
    public class ConfigurationTests
    {
        public const string TYPE_COLLECTOR = "Collector.SDK.Tests.Mocks.MockCollector, Core.Collector.SDK.Tests";
        public const string TYPE_STACK = "Collector.SDK.Tests.Mocks.MockStack, Core.Collector.SDK.Tests";
        public const string TYPE_CONVERTER = "Collector.SDK.Tests.Mocks.MockConverter, Core.Collector.SDK.Tests";
        public const string TYPE_READER = "Collector.SDK.Tests.Mocks.MockReader, Core.Collector.SDK.Tests";
        public const string TYPE_MAPPER = "Collector.SDK.Tests.Mocks.MockMapper, Core.Collector.SDK.Tests";
        public const string TYPE_TRANSFORMER = "Collector.SDK.Tests.Mocks.MockTransformer, Core.Collector.SDK.Tests";
        public const string TYPE_TRANSFORMATION_HANDLER = "Collector.SDK.Tests.Mocks.MockTransformationHandler, Core.Collector.SDK.Tests";
        public const string TYPE_PUBLISHER = "Collector.SDK.Tests.Mocks.MockPublisher, Core.Collector.SDK.Tests";
        public const string TYPE_DATA = "Collector.SDK.Tests.Mocks.MockEntity, Core.Collector.SDK.Tests";

        public static readonly string TYPE_READER_WITH_SINGLETON_DEPENDENCY = $"{typeof(MockReaderWithSingletonDependency).FullName}, {typeof(MockReaderWithSingletonDependency).Assembly.GetName().Name}";
        public const string NLOG_EXTRAS_ASSEMBLY = "AutoFac.Extras.NLog.DotNetCore";
        public const string COLLECTOR_SDK_ASSEMBLY = "Core.Collector.SDK";

        [TestInitialize]
        public void Init()
        {
            ComponentRegistration.Reset();

            ComponentRegistration.RegisterComponent<ICollector>(ConfigurationTests.TYPE_COLLECTOR);
            ComponentRegistration.RegisterComponent<IStack>(ConfigurationTests.TYPE_STACK);
            ComponentRegistration.RegisterComponent<IConverter>(ConfigurationTests.TYPE_CONVERTER);
            ComponentRegistration.RegisterComponent<IReader>(ConfigurationTests.TYPE_READER);
            ComponentRegistration.RegisterComponent<ITransformer>(ConfigurationTests.TYPE_TRANSFORMER);
            ComponentRegistration.RegisterComponent<ITransformedDataHandler>(ConfigurationTests.TYPE_TRANSFORMATION_HANDLER);
            ComponentRegistration.RegisterComponent<IPublisher>(ConfigurationTests.TYPE_PUBLISHER);
            ComponentRegistration.RegisterComponent<IMapper>(ConfigurationTests.TYPE_MAPPER);
            ComponentRegistration.RegisterComponent<IEntity>(ConfigurationTests.TYPE_DATA);

            ComponentRegistration.RegisterTypesFromAssembly(new ThirdPartyAutofacConfiguration(){
                AssemblyName = COLLECTOR_SDK_ASSEMBLY,
                Type = "LoggerFacade"
            });
            ComponentRegistration.RegisterModulesFromAssembly(new ThirdPartyAutofacConfiguration()
            {
                AssemblyName = NLOG_EXTRAS_ASSEMBLY,
                RegisterAll = true
            });

            ComponentRegistration.Build();
        }

        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void ComponentRegistration_Meaningful_Exception()
        {
            try
            {
                var collector = ComponentRegistration.CreateInstance<ICollector>("Something,Something");
                collector.Should().BeNull();
            }
            catch (Exception e)
            {
                e.Message.Should().Contain("The requested service 'Something (Collector.SDK.Collectors.ICollector)' has not been registered.");
            }
        }
        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        [ExpectedException(typeof(ApplicationException),  "After component registration reset, excpetion expected on create instance.")]
        public void ComponentRegistration_Reset_Success()
        {
            ComponentRegistration.Reset();
            var collector = ComponentRegistration.CreateInstance<ICollector>(ConfigurationTests.TYPE_COLLECTOR);
            collector.Should().NotBeNull();
        }
        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void ComponentRegistration_Build_Success()
        {
            ComponentRegistration.Reset();
            ComponentRegistration.RegisterComponent<ICollector>(ConfigurationTests.TYPE_COLLECTOR);
            ComponentRegistration.RegisterTypesFromAssembly(new ThirdPartyAutofacConfiguration()
            {
                AssemblyName = COLLECTOR_SDK_ASSEMBLY,
                Type = "LoggerFacade"
            });
            ComponentRegistration.RegisterModulesFromAssembly(new ThirdPartyAutofacConfiguration()
            {
                AssemblyName = NLOG_EXTRAS_ASSEMBLY,
                RegisterAll = true
            });
            ComponentRegistration.Build();

            var collector = ComponentRegistration.CreateInstance<ICollector>(ConfigurationTests.TYPE_COLLECTOR);
            collector.Should().NotBeNull();
        }
        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void MapperRegistration_Success()
        {
            ComponentRegistration.Reset();
            ComponentRegistration.RegisterComponent(CreateMapperConfig("1", "2"));
            ComponentRegistration.RegisterTypesFromAssembly(new ThirdPartyAutofacConfiguration()
            {
                AssemblyName = COLLECTOR_SDK_ASSEMBLY,
                Type = "LoggerFacade"
            });
            ComponentRegistration.RegisterModulesFromAssembly(new ThirdPartyAutofacConfiguration()
            {
                AssemblyName = NLOG_EXTRAS_ASSEMBLY,
                RegisterAll = true
            });
            ComponentRegistration.Build();
            var mapper = ComponentRegistration.CreateInstance<IMapper>(ConfigurationTests.TYPE_MAPPER);
            mapper.Should().NotBeNull();
        }
        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void CollectorRegistration_Success()
        {
            ComponentRegistration.Reset();
            ComponentRegistration.RegisterComponents(CreateCollectorConfig("1"));
            ComponentRegistration.RegisterTypesFromAssembly(new ThirdPartyAutofacConfiguration()
            {
                AssemblyName = COLLECTOR_SDK_ASSEMBLY,
                Type = "LoggerFacade"
            });
            ComponentRegistration.RegisterModulesFromAssembly(new ThirdPartyAutofacConfiguration()
            {
                AssemblyName = NLOG_EXTRAS_ASSEMBLY,
                RegisterAll = true
            });
            ComponentRegistration.Build();
            var collector = ComponentRegistration.CreateInstance<ICollector>(ConfigurationTests.TYPE_COLLECTOR);
            collector.Should().NotBeNull();
        }
        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void PublisherRegistration_Success()
        {
            var publishers = new List<PublisherConfiguration>();
            publishers.Add(CreatePublisherConfig("1", "2", "3"));

            ComponentRegistration.Reset();
            // the collector needs to be resgistered since it is injected
            ComponentRegistration.RegisterComponent<ICollector>(ConfigurationTests.TYPE_COLLECTOR);
            ComponentRegistration.RegisterComponents(publishers);
            ComponentRegistration.RegisterTypesFromAssembly(new ThirdPartyAutofacConfiguration()
            {
                AssemblyName = COLLECTOR_SDK_ASSEMBLY,
                Type = "LoggerFacade"
            });
            ComponentRegistration.RegisterModulesFromAssembly(new ThirdPartyAutofacConfiguration()
            {
                AssemblyName = NLOG_EXTRAS_ASSEMBLY,
                RegisterAll = true
            });
            ComponentRegistration.Build();
            var publisher = ComponentRegistration.CreateInstance<IPublisher>(ConfigurationTests.TYPE_PUBLISHER);
            publisher.Should().NotBeNull();
        }
        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void ReaderRegistration_Success()
        {
            var readers = new List<ReaderConfiguration>();
            readers.Add(CreateReaderConfig("1", "2"));

            ComponentRegistration.Reset();
            // the collector needs to be resgistered since it is injected
            ComponentRegistration.RegisterComponent<ICollector>(ConfigurationTests.TYPE_COLLECTOR);
            ComponentRegistration.RegisterComponents(readers);
            ComponentRegistration.RegisterTypesFromAssembly(new ThirdPartyAutofacConfiguration()
            {
                AssemblyName = COLLECTOR_SDK_ASSEMBLY,
                Type = "LoggerFacade"
            });
            ComponentRegistration.RegisterModulesFromAssembly(new ThirdPartyAutofacConfiguration()
            {
                AssemblyName = NLOG_EXTRAS_ASSEMBLY,
                RegisterAll = true
            });
            ComponentRegistration.Build();
            var reader = ComponentRegistration.CreateInstance<IReader>(ConfigurationTests.TYPE_READER);
            reader.Should().NotBeNull();
        }
        [TestMethod]
        [Owner("ken.thomas@ivanti.com")]
        [TestCategory("Unit")]
        public void SingletonRegistration_Build_Success()
        {
            var readers = new List<ReaderConfiguration>();
            readers.Add(CreateReaderWithSingletonDependencyConfig("1", "2"));

            ComponentRegistration.Reset();
            // the collector needs to be registered since it is injected
            ComponentRegistration.RegisterComponent<ICollector>(ConfigurationTests.TYPE_COLLECTOR);
            ComponentRegistration.RegisterComponents(readers);
            ComponentRegistration.RegisterTypesFromAssembly(new ThirdPartyAutofacConfiguration()
            {
                AssemblyName = COLLECTOR_SDK_ASSEMBLY,
                Type = "LoggerFacade"
            });
            ComponentRegistration.RegisterTypesFromAssembly(new ThirdPartyAutofacConfiguration()
            {
                AssemblyName = typeof(MockDisposableSingleton).Assembly.GetName().Name,
                Type = nameof(MockDisposableSingleton),
                Singleton = true
            });
            ComponentRegistration.RegisterModulesFromAssembly(new ThirdPartyAutofacConfiguration()
            {
                AssemblyName = NLOG_EXTRAS_ASSEMBLY,
                RegisterAll = true
            });
            ComponentRegistration.Build();
            var reader = ComponentRegistration.CreateInstance<IReader>(ConfigurationTests.TYPE_READER_WITH_SINGLETON_DEPENDENCY);
            reader.Should().NotBeNull();
        }
        [TestMethod]
        [Owner("ken.thomas@ivanti.com")]
        [TestCategory("Unit")]
        public void SingletonRegistration_Fails_Without_Singleton_Flag()
        {
            var readers = new List<ReaderConfiguration>();
            readers.Add(CreateReaderWithSingletonDependencyConfig("1", "2"));

            ComponentRegistration.Reset();
            // the collector needs to be registered since it is injected
            ComponentRegistration.RegisterComponent<ICollector>(ConfigurationTests.TYPE_COLLECTOR);
            ComponentRegistration.RegisterComponents(readers);
            ComponentRegistration.RegisterTypesFromAssembly(new ThirdPartyAutofacConfiguration()
            {
                AssemblyName = COLLECTOR_SDK_ASSEMBLY,
                Type = "LoggerFacade"
            });
            ComponentRegistration.RegisterModulesFromAssembly(new ThirdPartyAutofacConfiguration()
            {
                AssemblyName = NLOG_EXTRAS_ASSEMBLY,
                RegisterAll = true
            });
            ComponentRegistration.Build();

            IReader reader = null;
            Assert.ThrowsException<DependencyResolutionException>(() => reader = ComponentRegistration.CreateInstance<IReader>(ConfigurationTests.TYPE_READER_WITH_SINGLETON_DEPENDENCY));
            reader.Should().BeNull();
        }
        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void TransformerRegistration_Success()
        {
            var transformers = new List<TransformerConfiguration>();
            transformers.Add(CreateTransformerConfig("1", CreateMapperConfig("2", "1"), "3"));

            ComponentRegistration.Reset();
            // the collector needs to be resgistered since it is injected
            ComponentRegistration.RegisterComponent<ICollector>(ConfigurationTests.TYPE_COLLECTOR);
            ComponentRegistration.RegisterComponents(transformers);
            ComponentRegistration.RegisterTypesFromAssembly(new ThirdPartyAutofacConfiguration()
            {
                AssemblyName = COLLECTOR_SDK_ASSEMBLY,
                Type = "LoggerFacade"
            });
            ComponentRegistration.RegisterModulesFromAssembly(new ThirdPartyAutofacConfiguration()
            {
                AssemblyName = NLOG_EXTRAS_ASSEMBLY,
                RegisterAll = true
            });
            ComponentRegistration.Build();
            var reader = ComponentRegistration.CreateInstance<ITransformer>(ConfigurationTests.TYPE_TRANSFORMER);
            reader.Should().NotBeNull();
        }
        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void CollectorConfiguration_Constructed_Correctly()
        {
            var config = new CollectorConfiguration();
            config.Id = "1";
            config.Type = ConfigurationTests.TYPE_COLLECTOR;

            config.EndPoints.Should().NotBeNull();
            config.Publishers.Should().NotBeNull();
            config.Readers.Should().NotBeNull();
            config.Properties.Should().NotBeNull();
            config.Transformers.Should().NotBeNull();
            config.Id.Should().Be("1");
            config.Type.Should().Be(ConfigurationTests.TYPE_COLLECTOR);
        }

        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void CollectorFactory_CreateConverter_Succeeds()
        {
            var converter = CollectorFactory.CreateConverter(TYPE_CONVERTER);
            converter.Should().NotBeNull();
        }
        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void CollectorFactory_CreateReader_Succeeds()
        {
            var config = CreateEndPointConfig("1", "FileName", "c:\\temp\\temp.log");
            var handler = new MockHandler();
            var reader = CollectorFactory.CreateReader(TYPE_READER, "2", config, handler);
            reader.Should().NotBeNull();
        }
        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void CollectorFactory_CreateMapper_Succeeds()
        {
            var mapper = CollectorFactory.CreateMapper(CreateMapperConfig("3", "4"));
            mapper.Should().NotBeNull();
        }
        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void CollectorFactory_CreateTransformer_Succeeds()
        {
            var mapperConfigs = new List<MapperConfiguration>();

            var config = new TransformerConfiguration()
            {
                Id = "4",
                Type = TYPE_TRANSFORMER,
                Mappers = mapperConfigs,
                ReaderId = "2"
            };
            mapperConfigs.Add(CreateMapperConfig("3", config.Id));

            var handler = new MockTransformationHandler();
            var transformer = CollectorFactory.CreateTransformer(config, handler);
            transformer.Should().NotBeNull();
        }

        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void CollectorFactory_CreatePublisher_Succeeds()
        {
            var endpointConfig = CreateEndPointConfig("10", "URI", "http://acme.com/ivanti/uno/discovery");
            var config = new PublisherConfiguration()
            {
                Id = "4",
                Type = TYPE_PUBLISHER,
                EndpointId = "1",
                TransformerId = "4"
            };
            var handler = new MockTransformationHandler();
            var publisher = CollectorFactory.CreatePublisher(config, endpointConfig);
            publisher.Should().NotBeNull();
        }
        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void CollectorFactory_CreateCollector_Succeeds()
        {
            var collector = CollectorFactory.CreateCollector(CreateCollectorConfig("1000"));
            collector.Should().NotBeNull();
            collector.Id.Should().NotBeNullOrEmpty();
        }
        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void CollectorFactory_CreateStack_Succeeds()
        {
            var config = CreateCollectorConfig("1000");
            var stack = CollectorFactory.CreateStack("2", config);
            stack.Should().NotBeNull();
            stack.Reader.Id.Should().NotBe(config.Readers[0].Id);
            stack.Transformers[0].Id.Should().NotBe(config.Transformers[0].Id);
            stack.Publishers[0].Id.Should().NotBe(config.Publishers[0].Id);
        }
        /// <summary>
        /// Create a collector config.
        /// </summary>
        /// <param name="id">The id of the collector</param>
        /// <returns>A new config</returns>
        public static CollectorConfiguration CreateCollectorConfig(string id)
        {
            var endpointReaderConfig = CreateEndPointConfig("1", "FileName", "c:\\temp\\temp.log");
            var endpointPublisherConfig = CreateEndPointConfig("10", "URI", "http://acme.com/ivanti/uno/discovery");

            var readerConfig = CreateReaderConfig("2", endpointReaderConfig.Id);

            var transformerConfig = CreateTransformerConfig("4", CreateMapperConfig("3", "4"), readerConfig.Id);

            var endpoints = new List<EndPointConfiguration>();
            endpoints.Add(endpointReaderConfig);
            endpoints.Add(endpointPublisherConfig);

            var readers = new List<ReaderConfiguration>();
            readers.Add(readerConfig);

            var transformers = new List<TransformerConfiguration>();
            transformers.Add(transformerConfig);

            var publishers = new List<PublisherConfiguration>();
            publishers.Add(CreatePublisherConfig("5", endpointPublisherConfig.Id, transformerConfig.Id));

            return new CollectorConfiguration()
            {
                Id = id,
                EndPoints = endpoints,
                Readers = readers,
                Transformers = transformers,
                Publishers = publishers,
                Type = TYPE_COLLECTOR,
                StackType = TYPE_STACK
            };
        }
        /// <summary>
        /// Create a reader config.
        /// </summary>
        /// <param name="id">Id of the reader</param>
        /// <param name="endpointId">Id of the end point</param>
        /// <returns>A new config</returns>
        public static ReaderConfiguration CreateReaderConfig(string id, string endpointId)
        {
            return new ReaderConfiguration()
            {
                Id = id,
                EndpointId = endpointId,
                DataType = "Device",
                Type = TYPE_READER
            };

        }
        /// <summary>
        /// Create a reader config with a dependency.
        /// </summary>
        /// <param name="id">Id of the reader</param>
        /// <param name="endpointId">Id of the end point</param>
        /// <returns>A new config</returns>
        public static ReaderConfiguration CreateReaderWithSingletonDependencyConfig(string id, string endpointId)
        {
            return new ReaderConfiguration()
            {
                Id = id,
                EndpointId = endpointId,
                DataType = "Device",
                Type = ConfigurationTests.TYPE_READER_WITH_SINGLETON_DEPENDENCY
            };

        }
        /// <summary>
        /// Create a transformer config
        /// </summary>
        /// <param name="id">Id of the transformer</param>
        /// <param name="mapperConfig">Mapper</param>
        /// <param name="readerId">Id of the reader.</param>
        /// <returns>A new config</returns>
        public static TransformerConfiguration CreateTransformerConfig(string id, MapperConfiguration mapperConfig, string readerId)
        {
            var mapperConfigs = new List<MapperConfiguration>();
            mapperConfigs.Add(mapperConfig);

            return new TransformerConfiguration()
            {
                Id = id,
                Type = TYPE_TRANSFORMER,
                Mappers = mapperConfigs,
                ReaderId = readerId
            };
        }
        /// <summary>
        /// Create a publisher config.
        /// </summary>
        /// <param name="id">Id of the publisher</param>
        /// <param name="endpointId">Id of the end point</param>
        /// <param name="transformerId">Id of the transformer</param>
        /// <returns>A new config</returns>
        public static PublisherConfiguration CreatePublisherConfig(string id, string endpointId, string transformerId)
        {
            var config = new PublisherConfiguration()
            {
                Id = id,
                Type = TYPE_PUBLISHER,
                EndpointId = endpointId,
                TransformerId = transformerId
            };
            return config;
        }
        /// <summary>
        /// Create an end point config.
        /// </summary>
        /// <param name="id">The id of the config</param>
        /// <param name="propertyKey">The property key</param>
        /// <param name="propertyValue">The property value</param>
        /// <returns>A new config</returns>
        public static EndPointConfiguration CreateEndPointConfig(string id, string propertyKey, string propertyValue)
        {
            var properties = new Dictionary<string, string>();
            properties.Add(propertyKey, propertyValue);
            var config = new EndPointConfiguration
            {
                Id = id,
                User = "test",
                Password = "password",
                Properties = properties
            };
            return config;
        }
        /// <summary>
        /// Create a mapper config.
        /// </summary>
        /// <param name="id">The id of the mapper.</param>
        /// <param name="transformerId">The transformer id.</param>
        /// <returns>A new config</returns>
        public static MapperConfiguration CreateMapperConfig(string id, string transformerId)
        {
            var converters = new List<ConverterConfiguration>();
            converters.Add(new ConverterConfiguration()
            {
                LeftSideMap = new Dictionary<string, List<string>>() { { "foo", new List<string>() { "bar" } } },
                Type = TYPE_CONVERTER
            });
            var config = new MapperConfiguration()
            {
                Id = "3",
                Type = TYPE_MAPPER,
                TransformerId = "4",
                DataType = TYPE_DATA,
                Converters = converters
            };
            return config;
        }
    }
}
