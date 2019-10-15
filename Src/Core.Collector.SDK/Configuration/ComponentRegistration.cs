// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Autofac;
using Collector.SDK.Collectors;
using Collector.SDK.Converters;
using Collector.SDK.DataModel;
using Collector.SDK.Mappers;
using Collector.SDK.Publishers;
using Collector.SDK.Readers;
using Collector.SDK.Transformers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Collector.SDK.Configuration
{
    public static class ComponentRegistration
    {
        private static IContainer _container;
        private static ContainerBuilder _builder = new ContainerBuilder();
        private static bool _registered = false;

        public static bool Registered { get { return _registered; } set { _registered = value; } }

        /// <summary>
        /// Creates a new container builder.  This is for testing only.
        /// </summary>
        public static void Reset()
        {
            _container = null;
            _builder = new ContainerBuilder();
            _registered = false;
        }
        /// <summary>
        /// Register a class with Autofac.
        /// </summary>
        /// <param name="autofacConfig">Autfac config information</param>
        public static void RegisterTypesFromAssembly(ThirdPartyAutofacConfiguration autofacConfig)
        {
            var assembly = Assembly.Load(autofacConfig.AssemblyName);

            if (autofacConfig.RegisterAll) {
                _builder.RegisterAssemblyTypes(assembly);
            }
            else if (autofacConfig.Singleton)
            {
                _builder.RegisterAssemblyTypes(assembly)
                    .Where(t => t.Name.Equals(autofacConfig.Type))
                    .SingleInstance();
            }
            else
            {
                _builder.RegisterAssemblyTypes(assembly)
                    .Where(t => t.Name.Equals(autofacConfig.Type))
                    .AsImplementedInterfaces();
            }
        }
        /// <summary>
        /// Register a class with Autofac.
        /// </summary>
        /// <param name="autofacConfig">Autofac config info</param>
        public static void RegisterModulesFromAssembly(ThirdPartyAutofacConfiguration autofacConfig)
        {
            var assembly = Assembly.Load(autofacConfig.AssemblyName);
            _builder.RegisterAssemblyModules(assembly);
        }
        /// <summary>
        /// Register a class with Autofac.
        /// </summary>
        /// <typeparam name="T">The interface for the class</typeparam>
        /// <param name="type">The type of class as a string "[full class name], [assembly]"</param>
        public static void RegisterComponent<T>(string type)
        {
            var classAssembly = SplitClassAssembly(type);
            var assembly = Assembly.Load(classAssembly.Value);

            _builder.RegisterAssemblyTypes(assembly)
                .Keyed<T>(classAssembly.Key)
                .Where(t => t.FullName.Equals(classAssembly.Key))
                .As<T>();
        }
        /// <summary>
        /// Register a class with Autofac.
        /// </summary>
        /// <typeparam name="T">The interface for the class</typeparam>
        /// <param name="type">The type of class as a string "[full class name], [assembly]"</param>
        public static void RegisterSingletonComponent<T>(string type)
        {
            var classAssembly = SplitClassAssembly(type);
            var assembly = Assembly.Load(classAssembly.Value);

            _builder.RegisterAssemblyTypes(assembly)
                .Keyed<T>(classAssembly.Key)
                .Where(t => t.FullName.Equals(classAssembly.Key))
                .As<T>().SingleInstance();
        }
        /// <summary>
        /// Register a mapper, its converters and data entities.
        /// </summary>
        /// <param name="mapperConfig">The mapping configuration</param>
        public static void RegisterComponent(MapperConfiguration mapperConfig)
        {
            foreach (var converter in mapperConfig.Converters)
            {
                RegisterComponent<IConverter>(converter.Type);
            }
            foreach (var converter in mapperConfig.PipedConverters)
            {
                RegisterComponent<IConverter>(converter.Type);
            }
            if (mapperConfig.DataType != null)
            {
                RegisterComponent<IEntity>(mapperConfig.DataType);
            }
            RegisterComponent<IMapper>(mapperConfig.Type);
        }
        /// <summary>
        /// Register a transformer and associated mappers.
        /// </summary>
        /// <param name="transformerConfig">The transformer configuration.</param>
        public static void RegisterComponent(TransformerConfiguration transformerConfig)
        {
            foreach (var mapper in transformerConfig.Mappers)
            {
                RegisterComponent(mapper);
            }
            RegisterComponent<ITransformer>(transformerConfig.Type);
        }
        /// <summary>
        /// Register a publisher.
        /// </summary>
        /// <param name="publisherConfig">The publisher configuration.</param>
        public static void RegisterComponent(PublisherConfiguration publisherConfig)
        {
            RegisterComponent<IPublisher>(publisherConfig.Type);
        }
        /// <summary>
        /// Register a collection of readers.
        /// </summary>
        /// <param name="configs">A list of reader configurations.</param>
        public static void RegisterComponents(List<ReaderConfiguration> configs)
        {
            foreach (var readerConfig in configs)
            {
                RegisterComponent<IReader>(readerConfig.Type);
            }
        }
        /// <summary>
        /// Register a collection of transformers.
        /// </summary>
        /// <param name="configs">A list of transformer configurations.</param>
        public static void RegisterComponents(List<TransformerConfiguration> configs)
        {
            foreach (var transformerConfig in configs)
            {
                RegisterComponent(transformerConfig);
            }
        }
        /// <summary>
        /// Regsiter a list of publishers.
        /// </summary>
        /// <param name="configs">A list of publisher configuration.</param>
        public static void RegisterComponents(List<PublisherConfiguration> configs)
        {
            foreach (var publisherConfig in configs)
            {
                RegisterComponent(publisherConfig);
            }
        }
        /// <summary>
        /// Register a collector, which includes regsitering the entire "stack".
        /// </summary>
        /// <param name="collectorConfig">The collector configuration.</param>
        public static void RegisterComponents(CollectorConfiguration collectorConfig)
        {
            if (!_registered)
            {
                RegisterSingletonComponent<ICollector>(collectorConfig.Type);
                RegisterComponent<IStack>(collectorConfig.StackType);
                RegisterComponents(collectorConfig.Readers);
                RegisterComponents(collectorConfig.Transformers);
                RegisterComponents(collectorConfig.Publishers);

                foreach (var autofacConfig in collectorConfig.ThirdPartyModules)
                {
                    RegisterModulesFromAssembly(autofacConfig);
                }
                foreach (var autofacConfig in collectorConfig.ThirdPartyTypes)
                {
                    RegisterTypesFromAssembly(autofacConfig);
                }
            }
        }
        /// <summary>
        /// Build the autofac container.  This is for testing only.
        /// </summary>
        public static void Build()
        {
            if (!_registered)
            {
                // Should this be a part of the collector config?
                // RegisterModulesFromAssembly("Autofac.Extras.NLog");
                // _builder.RegisterModule<NLogModule>();
                // _builder.RegisterType<LoggerFacade>().As<Logging.ILogger>();
                // RegisterTypesFromAssembly("Collector.SDK", "LoggerFacade");

                _container = _builder.Build();
                _registered = true;
            }
        }
        /// <summary>
        /// Split the configured type to a pair [class name] -> [assembly name].
        /// </summary>
        /// <param name="type">The configured type string as "[full class name], [assembly name]"</param>
        /// <returns>A pair as [class name] -> [assembly name]</returns>
        private static KeyValuePair<string, string> SplitClassAssembly(string type)
        {
            var removedSpaces = type.Replace(" ", string.Empty);
            var types = removedSpaces.Split(new char[] { ',' });
            var className = types[0];
            var assemblyName = types[1];

            return new KeyValuePair<string, string>(className, assemblyName);
        }
        /// <summary>
        /// Creates an instance of a class based on an assembly.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T CreateInstance<T>(string type)
        {
            if (_container == null)
                throw new ApplicationException("The container is null, did you forget to build it?");

            using (var scope = _container.BeginLifetimeScope())
            {
                var pair = SplitClassAssembly(type);
                var component = scope.ResolveKeyed<T>(pair.Key);
                return component;
            }
            throw new ArgumentException("Unable to instantiate {0}", type);
        }
    }
}
