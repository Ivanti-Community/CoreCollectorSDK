// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System;
using System.Collections.Generic;
using System.IO;
using Collector.SDK.Collectors;
using Collector.SDK.Configuration;
using Collector.SDK.Converters;
using Collector.SDK.DataModel;
using Collector.SDK.Tests.Mocks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Collector.SDK.Tests
{
    [TestClass]
    public class PipedConverterTests
    {
        [TestInitialize]
        public void Init()
        {
            ComponentRegistration.Reset();
        }

        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void PipedConverters_PipedArray_Success()
        {
            // Use Case: A data point contains an array of objects (software items).
            // The PipedArrayConverter uses reflection to get each field, convert the field based on 
            // the configured converters for the associated source target mapping.  
            // The converter returns a dictionary of dictionaries based on an id.  
            // The id is determined in the SourceTargetConverter property "ArrayKey".
            // The property "PrefixId" means to prefix the id with the "ArrayKey", i.e. "Id_XXX"
            // where XXX is the Id field of the software.
            var items = CreateMockSoftwareArray();
            var row = new EntityCollection();
            row.Entities.Add("Items", items);

            var properties = new Dictionary<string, string>();
            properties.Add("ArrayKey", "Id");
            properties.Add("PrefixId", "true");

            var targetConverters = new List<SourceTargetConverter>();
            var mapping = new SourceTargetMapping()
            {
                //PipeData = true,
                Properties = properties,
                PrimaryKey = "Items",
                TargetConverters = targetConverters
            };
            var noOpConverter = new NoOpConverter();
            var leftSideMap = new Dictionary<string, List<string>>();
            leftSideMap.Add("Id", new List<string>() { "Id" });
            leftSideMap.Add("Title", new List<string>() { "Title" });
            leftSideMap.Add("DateTime", new List<string>() { "DateTime" });
            var noOpConfig = new ConverterConfiguration()
            {
                Id = "1",
                LeftSideMap = leftSideMap
            };
            noOpConverter.Configure(noOpConfig);

            var converters = new List<IConverter>();
            converters.Add(noOpConverter);

            var converter = new PipedArrayConverter();
            converters.Add(converter);

            var config = new ConverterConfiguration()
            {
                Id = "2",
                Mapping = mapping,
                PipedConverters = converters,
                Properties = properties
            };
            converter.Configure(config);
            var convertedData = converter.Convert(new KeyValuePair<string, object>("Items", items), row);
            convertedData.Count.Should().Be(2);

            foreach (var id in convertedData.Keys)
            {
                if (id.Contains("_pkAttrName"))
                    continue;
                var software = convertedData[id] as List<object>;
                var software0 = software[0] as Dictionary<string, object>;
                software0["Id"].Should().Be("1");
                software0["Title"].Should().Be("Some Software Title #1");
                software0["DateTime"].Should().NotBeNull();
                var software1 = software[1] as Dictionary<string, object>;
                software1["Id"].Should().Be("2");
                software1["Title"].Should().Be("Some Software Title #2");
                software1["DateTime"].Should().NotBeNull();
            }
        }

        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void PipedConverters_ConvertObjectArray_Success()
        {
            // Use Case: Same as PipedConverters_PipedArray_Success test except that
            // we are using a collector configuration file, creating a stack and then
            // invoking the configured transformer testing for converted a list of software items.
            using (StreamReader file = File.OpenText(@"Configuration\collector-config-array-converter-2.json"))
            {
                var serializer = new JsonSerializer();

                var collectorConfig = (CollectorConfiguration)serializer.Deserialize(file, typeof(CollectorConfiguration));
                var collector = CollectorFactory.CreateCollector(collectorConfig) as AbstractCollector;

                var stack = collector.CreateStack("31");
                var transformers = stack.Transformers;
                transformers.Count.Should().Be(1);

                var items = CreateMockSoftwareArray();
                var data = new List<DataModel.IEntityCollection>();
                var col = new EntityCollection();
                col.Entities.Add("Items", items);
                data.Add(col);

                var context = new Dictionary<string, string>();
                transformers[0].HandleData("31", data, context).Wait();

                stack.Publishers.Count.Should().Be(1);
                var publisher = stack.Publishers[0] as MockPublisher;
                publisher.Invoked.Should().BeTrue();

                publisher.Data.Count.Should().Be(1);
                var entities = publisher.Data[0] as IEntityCollection;
                entities.Entities.Count.Should().Be(2);

                var software = entities.Entities["Items"] as List<object>;
                software.Should().NotBeNull();
                var software0 = software[0] as Dictionary<string, object>;
                software0["Id"].Should().Be("1");
                software0["Title"].Should().Be("Some Software Title #1");
                software0["DateTimeUTC"].Should().NotBeNull();
                var software1 = software[1] as Dictionary<string, object>;
                software1["Id"].Should().Be("2");
                software1["Title"].Should().Be("Some Software Title #2");
                software1["DateTimeUTC"].Should().NotBeNull();
            }
        }

        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void PipedConverters_ConvertDelimitedDataPoint_Success()
        {
            // Use Case: A data point is a comma delimited list of data.
            // Converter converts a single data point into multiple data points.
            // The converter then pipes that into another set of configured converters.
            // The result is a single software item to be published.
            using (StreamReader file = File.OpenText(@"Configuration\collector-config-array-converter.json"))
            {
                var serializer = new JsonSerializer();

                var collectorConfig = (CollectorConfiguration)serializer.Deserialize(file, typeof(CollectorConfiguration));
                var collector = CollectorFactory.CreateCollector(collectorConfig) as AbstractCollector;

                var stack = collector.CreateStack("31");
                var transformers = stack.Transformers;
                transformers.Count.Should().Be(1);

                var data = new List<DataModel.IEntityCollection>();
                var col = new EntityCollection();
                col.Entities.Add("Items", "Some Software Title,01/17/2018,01:23:03.000,00000-789-1234567890");
                data.Add(col);

                var context = new Dictionary<string, string>();
                transformers[0].HandleData("31", data, context).Wait();

                stack.Publishers.Count.Should().Be(1);
                var publisher = stack.Publishers[0] as MockPublisher;
                publisher.Invoked.Should().BeTrue();

                publisher.Data.Count.Should().Be(1);
                var software = publisher.Data[0] as MockSoftware;
                software.Id.Should().NotBeNull();
                software.Title.Should().NotBeNull();
            }
        }

        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void PipedConverters_NoExceptions()
        {
            using (StreamReader file = File.OpenText(@"Configuration\collector-config-piped-converter.json"))
            {
                var serializer = new JsonSerializer();

                var collectorConfig = (CollectorConfiguration)serializer.Deserialize(file, typeof(CollectorConfiguration));
                var collector = CollectorFactory.CreateCollector(collectorConfig);
                collector.Run().Wait();
            }
        }

        [TestMethod]
        [Owner("kristen.may@ivanti.com")]
        [TestCategory("Unit")]
        public void PipedConverters_ConvertDeviceArray_Success()
        {
            using (StreamReader file = File.OpenText(@"Configuration\collector-config-device-converter.json"))
            {
                var serializer = new JsonSerializer();

                var collectorConfig = (CollectorConfiguration)serializer.Deserialize(file, typeof(CollectorConfiguration));
                var collector = CollectorFactory.CreateCollector(collectorConfig) as AbstractCollector;

                var stack = collector.CreateStack("31");
                var transformers = stack.Transformers;
                transformers.Count.Should().Be(1);

                var items = CreateMockDeviceArray();
                var data = new List<DataModel.IEntityCollection>();
                var col = new EntityCollection();
                col.Entities.Add("Items", items);
                data.Add(col);

                var context = new Dictionary<string, string>();
                transformers[0].HandleData("31", data, context).Wait();

                stack.Publishers.Count.Should().Be(1);
                var publisher = stack.Publishers[0] as MockPublisher;
                publisher.Invoked.Should().BeTrue();

                publisher.Data.Count.Should().Be(1);
                var entities = publisher.Data[0] as IEntityCollection;

                var convertedJson = JsonConvert.SerializeObject(entities.Entities);
                entities.Entities.Count.Should().Be(2);
            }
        }

        /// <summary>
        /// Create an array of Devices for testing.
        /// </summary>
        /// <returns>an array of MockSoftware</returns>
        private List<MockDevice> CreateMockDeviceArray()
        {
            var items = new List<MockDevice>();
            items.Add(new MockDevice()
            {
                DeviceId = "3d45fa69-b1b1-4049-abad-d685a1d4f375",
                Software = new List<MockSoftware>() {
                    new MockSoftware()
                    {
                    Id = "1",
                    Title = "Some Software Title #1",
                    DateTime = DateTime.Parse("01/17/2018 01:23:03.000")
                    },
                    new MockSoftware()
                    {
                        Id = "2",
                        Title = "Some Software Title #1",
                        DateTime = DateTime.Parse("01/17/2018 01:23:03.000")
                    }
                },
                Network = new MockNetwork()
                {
                    Address = "51.100.52.53",
                    BoundAdapters = new List<MockBoundAdapter>()
                    {
                        new MockBoundAdapter()
                        {
                            IPAddress = "51.100.52.53",
                            SubnetAddress = "255.255.0.0"
                        },
                        new MockBoundAdapter()
                        {
                            IPAddress = "22.103.51.60",
                            SubnetAddress = "255.255.0.0"
                        },
                    }
                }
            });

            items.Add(new MockDevice()
            {
                DeviceId = "22222222-b1b1-4049-abad-d685a1d4f375",
                Software = new List<MockSoftware>() {
                    new MockSoftware()
                    {
                        Id = "3",
                        Title = "Some Software Title #3",
                        DateTime = DateTime.Parse("01/17/2018 01:23:03.000")
                    },
                    new MockSoftware()
                    {
                        Id = "4",
                        Title = "Some Software Title #4",
                        DateTime = DateTime.Parse("01/17/2018 01:23:03.000")
                    }
                },
                Network = new MockNetwork()
                {
                    Address = "60.600.62.63",
                    BoundAdapters = new List<MockBoundAdapter>()
                    {
                        new MockBoundAdapter()
                        {
                            IPAddress = "60.600.62.63",
                            SubnetAddress = "255.255.0.0"
                        },
                        new MockBoundAdapter()
                        {
                            IPAddress = "60.600.62.63",
                            SubnetAddress = "255.255.0.0"
                        },
                    }
                }
            });
            return items;
        }

        /// <summary>
        /// Create an array of MockSoftware for testing.
        /// </summary>
        /// <returns>an array of MockSoftware</returns>
        private List<MockSoftware> CreateMockSoftwareArray()
        {
            var items = new List<MockSoftware>();
            items.Add(new MockSoftware()
            {
                Id = "1",
                Title = "Some Software Title #1",
                DateTime = DateTime.Parse("01/17/2018 01:23:03.000")
            });
            items.Add(new MockSoftware()
            {
                Id = "2",
                Title = "Some Software Title #2",
                DateTime = DateTime.Parse("01/17/2018 01:23:03.000")
            });
            return items;
        }
    }
}
