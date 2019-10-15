// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.Configuration;
using Collector.SDK.Mappers;
using Collector.SDK.Publishers;
using Collector.SDK.Readers;
using System;
using System.Collections.Generic;
using System.Reflection;
using Collector.SDK.Transformers;
using Collector.SDK.DataModel;
using System.Globalization;
using Collector.SDK.Converters;
using NLog;

namespace Collector.SDK.Collectors
{
    public class CollectorFactory
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Create a Collector, this involved creating all the layers - Readers, Transformers and Publishers.
        /// </summary>
        /// <param name="collectorConfig">The configuration that defines all the layers.</param>
        /// <param name="mapperConfigs">The mappers to use by transformers.</param>
        /// <returns></returns>
        public static ICollector CreateCollector(CollectorConfiguration collectorConfig)
        {
            ComponentRegistration.RegisterComponents(collectorConfig);
            ComponentRegistration.Build();

            var collector = ComponentRegistration.CreateInstance<ICollector>(collectorConfig.Type);
            collector.Configure(collectorConfig);
            return collector;
        }
        /// <summary>
        /// Create an entire stack.
        /// </summary>
        /// <param name="readerId"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IStack CreateStack(string readerId, CollectorConfiguration config)
        {
            var stack = ComponentRegistration.CreateInstance<IStack>(config.StackType);

            foreach (var readerConfig in config.Readers)
            {
                if (readerConfig.Id == readerId)
                {
                    var newReaderId = Guid.NewGuid().ToString();

                    List<IPublisher> publishers = new List<IPublisher>();
                    List<ITransformer> transformers = new List<ITransformer>();
                    foreach (var transformerConfig in config.Transformers)
                    {
                        if (transformerConfig.ReaderId == readerId)
                        {
                            var newTransformerId = Guid.NewGuid().ToString();
                            foreach (var publisherConfig in config.Publishers)
                            {
                                if (publisherConfig.TransformerId == transformerConfig.Id)
                                {
                                    foreach (var endpoint in config.EndPoints)
                                    {
                                        if (endpoint.Id == publisherConfig.EndpointId)
                                        {
                                            var newEndpoint = new EndPointConfiguration()
                                            {
                                                Id = Guid.NewGuid().ToString(),
                                                Password = endpoint.Password,
                                                User = endpoint.User
                                            };
                                            foreach (var key in endpoint.Properties.Keys)
                                            {
                                                newEndpoint.Properties.Add(key, endpoint.Properties[key]);
                                            }

                                            var newPublisherConfig = new PublisherConfiguration()
                                            {
                                                Id = Guid.NewGuid().ToString(),
                                                EndpointId = newEndpoint.Id,
                                                TransformerId = newTransformerId,
                                                Type = publisherConfig.Type
                                            };
                                            var publisher = CreatePublisher(newPublisherConfig, endpoint);
                                            publishers.Add(publisher);
                                        }
                                    }
                                }
                            }
                            var newTransformerConfig = new TransformerConfiguration()
                            {
                                Id = newTransformerId,
                                Type = transformerConfig.Type,
                                ReaderId = newReaderId,
                            };
                            foreach (var mapper in transformerConfig.Mappers)
                            {
                                var newMapperConfig = new MapperConfiguration()
                                {
                                    Id = mapper.Id,
                                    DataType = mapper.DataType,
                                    TransformerId = newTransformerId,
                                    Type = mapper.Type,
                                    SourceTargetMappings = mapper.SourceTargetMappings
                                };
                                foreach (var converter in mapper.PipedConverters)
                                {
                                    var newConverter = CopyConfig(converter);
                                    newMapperConfig.PipedConverters.Add(converter);
                                }
                                foreach (var converter in mapper.Converters)
                                {
                                    var newConverter = CopyConfig(converter);
                                    newMapperConfig.Converters.Add(converter);
                                }
                                newTransformerConfig.Mappers.Add(newMapperConfig);
                            }
                            var transformer = CreateTransformer(newTransformerConfig, stack);
                            transformers.Add(transformer);
                        }
                    }
                    foreach (var endpoint in config.EndPoints)
                    {
                        if (readerConfig.EndpointId.Equals(endpoint.Id))
                        {
                            var newEndpoint = new EndPointConfiguration()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Password = endpoint.Password,
                                User = endpoint.User
                            };
                            foreach (var key in endpoint.Properties.Keys)
                            {
                                newEndpoint.Properties.Add(key, endpoint.Properties[key]);
                            }
                            var reader = CreateReader(readerConfig.Type, newReaderId, newEndpoint, stack);
                            stack.Configure(reader, transformers, publishers);
                            break;
                        }
                    }
                    break;
                }
            }
            return stack;
        }
        private static ConverterConfiguration CopyConfig(ConverterConfiguration config)
        {
            var newConfig = new ConverterConfiguration()
            {
                Id = config.Id,
                LeftSideMap = config.LeftSideMap,
                Type = config.Type,
            };
            foreach (var key in config.Properties.Keys)
            {
                newConfig.Properties.Add(key, config.Properties[key]);
            }
            return newConfig;
        }
        /// <summary>
        /// Create an array of collectors
        /// </summary>
        /// <param name="collectorConfigs">An array of collector configurations</param>
        /// <returns>a list of collectors</returns>
        public static List<ICollector> CreateCollectors(CollectorConfiguration[] collectorConfigs)
        {
            var result = new List<ICollector>();
            foreach (var config in collectorConfigs)
            {
                ComponentRegistration.RegisterComponents(config);
            }
            ComponentRegistration.Build();
            foreach (var config in collectorConfigs)
            {
                var collector = ComponentRegistration.CreateInstance<ICollector>(config.Type);
                collector.Configure(config);
                result.Add(collector);
            }
            return result;
        }
        /// <summary>
        /// Create an Entity with the given fields
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="dataPoints"></param>
        /// <returns></returns>
        public static IEntity CreateEntity(string dataType, Dictionary<string, object> dataPoints)
        {
            var entity = ComponentRegistration.CreateInstance<IEntity>(dataType);
            if (entity != null)
            {
                var type = entity.GetType();
                foreach (var point in dataPoints)
                {
                    if (type.GetProperty(point.Key, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static) != null)
                    {
                        type.GetProperty(point.Key).SetValue(entity, point.Value);
                    }
                }
            }
            return entity;
        }
        /// <summary>
        /// Create a mapper to use to transform data.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IMapper CreateMapper(MapperConfiguration mapperConfig)
        {
            var mapper = ComponentRegistration.CreateInstance<IMapper>(mapperConfig.Type);
            if (mapper == null)
                throw new NullReferenceException("Unable to create Collector.  Invalid Mapper defined.");

            var converters = new Dictionary<string, IConverter>();
            if (mapperConfig.SourceTargetMappings != null && mapperConfig.SourceTargetMappings.Count > 0)
            {
                foreach (var converterConfig in mapperConfig.PipedConverters)
                {
                    var converter = ComponentRegistration.CreateInstance<IConverter>(converterConfig.Type);
                    if (converter == null)
                        throw new NullReferenceException("Unable to create Collector.  Invalid Converter defined.");

                    converter.Configure(converterConfig);
                    converters.Add(converter.Id, converter);
                }
            }
            else
            {
                foreach (var converterConfig in mapperConfig.Converters)
                {
                    var converter = ComponentRegistration.CreateInstance<IConverter>(converterConfig.Type);
                    if (converter == null)
                        throw new NullReferenceException("Unable to create Collector.  Invalid Converter defined.");

                    converter.Configure(converterConfig);
                    foreach (var key in converterConfig.LeftSideMap.Keys)
                    {
                        converters.Add(key, converter);
                    }
                }
            }
            mapper.Configure(mapperConfig, converters);
            return mapper;
        }
        /// <summary>
        /// Create a publisher.
        /// </summary>
        /// <param name="type">Fully qualified name of the publisher class</param>
        /// <param name="id">Id of the publisher</param>
        /// <param name="config">The config parameters for connecting to the data end point</param>
        /// <returns>A new publisher</returns>
        public static IPublisher CreatePublisher(PublisherConfiguration config, EndPointConfiguration endpointConfig )
        {
            try
            {
                var publisher = ComponentRegistration.CreateInstance<IPublisher>(config.Type);
                publisher.Configure(config.Id, endpointConfig);
                return publisher;
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
            return null;
        }

        public static IPublisher ClonePublisher(string publisherId, IPublisher publisher)
        {
            try
            {
                var assemblyName = publisher.GetType().Assembly.GetName().Name;
                var type = string.Format(CultureInfo.InvariantCulture, "{0},{1}",
                    publisher.GetType().FullName,
                    assemblyName);

                var newPublisher = ComponentRegistration.CreateInstance<IPublisher>(type);
                newPublisher.Configure(publisherId, publisher.EndPointConfig);
                return newPublisher;
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
            return null;
        }
        /// <summary>
        /// Create a connecter with a state change handler that will get invoked when the connecter's state changes.
        /// </summary>
        /// <param name="type">Fully qualified name of the connecter class and assembly, comma seperated.</param>
        /// <param name="id">The id of the connector.</param>
        /// <param name="config">The config parameters for connecting to the data source</param>
        /// <param name="handler">The handler to signal with blocks of data</param>
        /// <returns>A new connecter</returns>
        public static IReader CreateReader(string type, string id, EndPointConfiguration config, IDataHandler handler)
        {
            try
            {
                var connector = ComponentRegistration.CreateInstance<IReader>(type);
                connector.Configure(id, config, handler);
                return connector;
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
            return null;
        }
        /// <summary>
        /// Create a copy of a reader.
        /// </summary>
        /// <param name="id">The id to use for the cloned reader.</param>
        /// <param name="reader">The reader to copy.</param>
        /// <returns></returns>
        public static IReader CloneReader(string id, IReader reader)
        {
            try
            {
                var assemblyName =  reader.GetType().Assembly.GetName().Name;
                var type = string.Format(CultureInfo.InvariantCulture, "{0},{1}", 
                    reader.GetType().FullName, 
                    assemblyName);
                return CreateReader(type, id, reader.EndPointConfig, reader.Handler);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
            return null;
        }

        /// <summary>
        /// Create a Transformer.
        /// </summary>
        /// <param name="type">The name of the class to create.</param>
        /// <param name="id">The id of the transformer.</param>
        /// <param name="publishers">The publishers to use to publish the JSON data to a repository.</param>
        /// <param name="mappers">The mappers to use to transform/map the data.</param>
        /// <returns>A new handler.</returns>
        public static ITransformer CreateTransformer(TransformerConfiguration config, ITransformedDataHandler handler)
        {
            var mappers = new List<IMapper>();
            foreach (var mapperConfig in config.Mappers)
            {
                var mapper = CollectorFactory.CreateMapper(mapperConfig);
                if (mapper == null)
                    throw new NullReferenceException("Unable to create Collector.  Invalid Mapper defined.");

                mappers.Add(mapper);
            }
            var transformer = ComponentRegistration.CreateInstance<ITransformer>(config.Type);
            transformer.Configure(config, handler);
            return transformer;
        }
        /// <summary>
        /// Create a copy of a transformer.
        /// </summary>
        /// <param name="id">New id of the transformer, must be unique</param>
        /// <param name="transformer">The transformer to copy.</param>
        /// <returns>A new transformer</returns>
        public static ITransformer CloneTransformer(string id, ITransformer transformer)
        {
            try
            {
                var newConfig = new TransformerConfiguration()
                {
                    Id = id,
                    ReaderId = transformer.Config.ReaderId,
                    Mappers = transformer.Config.Mappers,
                    Type = transformer.Config.Type,
                };
                var mappers = new List<IMapper>();
                foreach (var mapperConfig in newConfig.Mappers)
                {
                    var mapper = CollectorFactory.CreateMapper(mapperConfig);
                    if (mapper == null)
                        throw new NullReferenceException("Unable to create Collector.  Invalid Mapper defined.");

                    mappers.Add(mapper);
                }
                var newTransformer = ComponentRegistration.CreateInstance<ITransformer>(newConfig.Type);
                newTransformer.Configure(newConfig, transformer.Handler);
                return transformer;
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
            return null;
        }
        /// <summary>
        /// Create a converter.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IConverter CreateConverter(string type)
        {
            try
            {
                var converter = ComponentRegistration.CreateInstance<IConverter>(type);
                return converter;
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
            return null;
        }
        /// <summary>
        /// Clone a converter.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IConverter CloneConverter(IConverter converter)
        {
            try
            {
                var assemblyName = converter.GetType().Assembly.GetName().Name;
                var type = string.Format(CultureInfo.InvariantCulture, "{0},{1}",
                    converter.GetType().FullName,
                    assemblyName);
                return CreateConverter(type);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
            return null;
        }
    }
}
