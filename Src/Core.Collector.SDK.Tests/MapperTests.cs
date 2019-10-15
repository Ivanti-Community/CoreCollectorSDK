// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System;
using System.Collections.Generic;
using Collector.SDK.Configuration;
using Collector.SDK.Converters;
using Collector.SDK.DataModel;
using Collector.SDK.Mappers;
using Collector.SDK.Tests.Mocks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Collector.SDK.Tests
{
    [TestClass]
    public class MapperTests
    {
        private const string ID_CONVERTER_1 = "10";
        private const string ID_CONVERTER_2 = "11";

        private const string TYPE_MOCKUSER = "Collector.SDK.Tests.Mocks.MockUser,Core.Collector.SDK.Tests";
        private const string TYPE_DATE_TIME_COMBINE_CONVERTER = "Collector.SDK.Converters.CombineDateTimeConverter,Core.Collector.SDK";
        private const string TYPE_DATE_TIME_UTC_CONVERTER = "Collector.SDK.Converters.DateTimeUtcConverter,Core.Collector.SDK";
        private const string TYPE_UPPER_CASE_CONVERTER = "Collector.SDK.Converters.AllUpperCaseConverter,Core.Collector.SDK";

        [TestInitialize]
        public void Init()
        {
            ComponentRegistration.Reset();
            ComponentRegistration.RegisterComponent<IEntity>(TYPE_MOCKUSER);
            ComponentRegistration.RegisterComponent<IConverter>(TYPE_DATE_TIME_COMBINE_CONVERTER);
            ComponentRegistration.RegisterComponent<IConverter>(TYPE_DATE_TIME_UTC_CONVERTER);
            ComponentRegistration.RegisterComponent<IConverter>(TYPE_UPPER_CASE_CONVERTER);
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
        public void DictionaryMapper_Success()
        {
            var converters = new Dictionary<string, IConverter>();
            var converterConfig1 = new ConverterConfiguration()
            {
                Id = ID_CONVERTER_1,
            };
            var converter1 = new AllUpperCaseConverter();
            converter1.Configure(converterConfig1);
            converters.Add(converter1.Id, converter1);

            var leftSideMap1 = new Dictionary<string, List<string>>()
            {
                { "FirstName", new List<string>() { "FirstName" } }
            };

            // Create the converters we are targeting
            var targetConverters = new List<SourceTargetConverter>();
            targetConverters.Add(new SourceTargetConverter() { Id = ID_CONVERTER_1, LeftSideMap = leftSideMap1, CombineInputOutput = false });
            // Now create the targeted mapping
            var targetMappings = new List<SourceTargetMapping>();
            targetMappings.Add(new SourceTargetMapping()
            {
                PrimaryKey = "FirstName",
                TargetConverters = targetConverters
            });
            var mapperConfig = new MapperConfiguration()
            {
                Id = "1234",
                TransformerId = "6678",
                SourceTargetMappings = targetMappings,
                DataType = TYPE_MOCKUSER
            };
            var mapper = new DictionaryMapper();
            mapper.Configure(mapperConfig, converters);

            var dataToConvert = new Dictionary<string, object>(){
                { "FirstName", "Jane" },
                { "LastName", "Doe" },
                { "Email", "jane.doe@acme.com" },
                { "LastLogin",  "06-25-2018 09:03:45.123456" }
            };
            var dataPoint = new KeyValuePair<string, object>("AD User", dataToConvert);
            var dataRow = new EntityCollection();
            dataRow.Entities.Add(dataPoint.Key, dataPoint.Value);

            var data = mapper.Map(new List<IEntityCollection> { dataRow });

            data.Count.Should().Be(1);
            var user = data[0] as MockUser;
            user.FirstName.Should().Be("JANE");
        }

        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void DefaultMapper_PipeConverters_Map_Success()
        {
            // Use Case : I want to first combine a date and time field.  Then pipe
            // that result into the UTC converter.  The end result should be one entry 
            // that is the date and time combined, converted to UTC.

            var converters = CreateConverters();

            var collector = new MockCollector(new MockLogger());
            collector.Configure(new CollectorConfiguration()
            {
                Id = "1",
                Version = 2.0
            });

            var mapper = new DefaultMapper(new MockLogger(), collector);

            var leftSideMap1 = new Dictionary<string, List<string>>()
            {
                { "Date", new List<string>() { "DateTime" } }
            };
            var leftSideMap2 = new Dictionary<string, List<string>>()
            {
                { "DateTime", new List<string>() { "DateTimeUTC" } }
            };

            // Create the converters we are targeting
            var targetConverters = new List<SourceTargetConverter>();
            targetConverters.Add(new SourceTargetConverter() { Id = ID_CONVERTER_1, LeftSideMap = leftSideMap1, CombineInputOutput = false });
            targetConverters.Add(new SourceTargetConverter() { Id = ID_CONVERTER_2, LeftSideMap = leftSideMap2, CombineInputOutput = false});
            // Now create the targeted mapping
            var targetMappings = new List<SourceTargetMapping>();
            targetMappings.Add(new SourceTargetMapping()
            {
                PrimaryKey = "Date",
                TargetConverters = targetConverters
            });
            // Finally create the mapper config and configure the mapper
            var mapperConfig = new MapperConfiguration()
            {
                Id = "1234",
                TransformerId = "6678",
                SourceTargetMappings = targetMappings
            };
            mapper.Configure(mapperConfig, converters);

            mapper.Id.Should().Be("1234");
            mapper.TransformerId.Should().Be("6678");

            var dataRow = new EntityCollection();
            dataRow.Entities.Add("Date", "1/17/2018");
            dataRow.Entities.Add("Time", "15:46:07.000");

            var data = new List<IEntityCollection>();
            data.Add(dataRow);

            var convertedData = mapper.Map(data);
            convertedData.Should().NotBeNull();
            convertedData.Count.Should().Be(1);

            var entities = convertedData[0] as IEntityCollection;
            entities.Entities.Count.Should().Be(1);
            entities.Entities.ContainsKey("DateTimeUTC").Should().BeTrue();

            var dateTime = DateTime.Parse(entities.Entities["DateTimeUTC"].ToString());
            var expectedDateTime = DateTime.Parse("1/17/2018 15:46:07.000");
            dateTime.Month.Should().Be(expectedDateTime.Month);
            dateTime.Day.Should().Be(expectedDateTime.Day);
            dateTime.Year.Should().Be(expectedDateTime.Year);
            dateTime.Hour.Should().Be(expectedDateTime.Hour);
            dateTime.Minute.Should().Be(expectedDateTime.Minute);
        }

        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void DefaultMapper_NoPipeConverters_Map_Success()
        {
            // Use Case : I want to first combine a date and time field.  Then pipe
            // that result into the UTC converter along with the original data point.  
            // Because PipedData is set to false then the end result should be two entries.  
            // 1. Combined Date and Time
            // 2. Combined Date and Time converted to UTC.

            var converters = CreateConverters();

            var collector = new MockCollector(new MockLogger());
            collector.Configure(new CollectorConfiguration()
            {
                Id = "1",
                Version = 2.0
            });

            var mapper = new DefaultMapper(new MockLogger(), collector);

            var leftSideMap1 = new Dictionary<string, List<string>>()
            {
                { "Date", new List<string>() { "DateTime" } }
            };
            var leftSideMap2 = new Dictionary<string, List<string>>()
            {
                { "DateTime", new List<string>() { "DateTimeUTC" } }
            };

            // Create the converters we are targeting
            var targetConverters = new List<SourceTargetConverter>();
            targetConverters.Add(new SourceTargetConverter() { Id = ID_CONVERTER_1, LeftSideMap = leftSideMap1, CombineInputOutput = false });
            targetConverters.Add(new SourceTargetConverter() { Id = ID_CONVERTER_2, LeftSideMap = leftSideMap2, CombineInputOutput = true });
            // Now create the targeted mapping
            var targetMappings = new List<SourceTargetMapping>();
            targetMappings.Add(new SourceTargetMapping()
            {
                PrimaryKey = "Date",
                TargetConverters = targetConverters
            });
            // Finally create the mapper config and configure the mapper
            var mapperConfig = new MapperConfiguration()
            {
                Id = "1234",
                TransformerId = "6678",
                SourceTargetMappings = targetMappings
            };
            mapper.Configure(mapperConfig, converters);

            mapper.Id.Should().Be("1234");
            mapper.TransformerId.Should().Be("6678");

            var dataRow = new EntityCollection();
            dataRow.Entities.Add("Date", "1/17/2018");
            dataRow.Entities.Add("Time", "15:46:07.000");

            var data = new List<IEntityCollection>();
            data.Add(dataRow);

            var convertedData = mapper.Map(data);
            convertedData.Should().NotBeNull();
            convertedData.Count.Should().Be(1);

            var entities = convertedData[0] as IEntityCollection;
            entities.Entities.Count.Should().Be(2);
            entities.Entities.ContainsKey("DateTimeUTC").Should().BeTrue();

            var dateTime = DateTime.Parse(entities.Entities["DateTimeUTC"].ToString());
            var expectedDateTime = DateTime.Parse("1/17/2018 15:46:07.000");
            dateTime.Month.Should().Be(expectedDateTime.Month);
            dateTime.Day.Should().Be(expectedDateTime.Day);
            dateTime.Year.Should().Be(expectedDateTime.Year);
            dateTime.Hour.Should().Be(expectedDateTime.Hour);
            dateTime.Minute.Should().Be(expectedDateTime.Minute);
        }

        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
        [TestCategory("Unit")]
        public void VersionOneMapper_Map_Success()
        {
            var config = new ConverterConfiguration()
            {
                LeftSideMap = new Dictionary<string, List<string>>() { { "ABC", new List<string>() { "XYZ" } } },
                Properties = new Dictionary<string, string>()
            };
            var converter = new AllUpperCaseConverter();
            converter.Configure(config);

            var converters = new Dictionary<string, IConverter>();
            converters.Add("ABC", converter);


            var collector = new MockCollector(new MockLogger());
            collector.Configure(new CollectorConfiguration()
            {
                Id = "1",
                Version = 1.0
            });

            var mapper = new VersionOneMapper();
            mapper.Configure("1", null, "2", converters);

            mapper.Id.Should().Be("1");
            mapper.TransformerId.Should().Be("2");

            var dataPoint = new KeyValuePair<string, object>("ABC", "foobar");
            var dataRow = new EntityCollection();
            dataRow.Entities.Add(dataPoint.Key, dataPoint.Value);

            var data = new List<IEntityCollection>();
            data.Add(dataRow);

            var convertedData = mapper.Map(data);
            convertedData.Should().NotBeNull();
        }

        private Dictionary<string, IConverter> CreateConverters()
        {
            var config1 = new ConverterConfiguration() { Id = ID_CONVERTER_1 };
            var converter1 = new CombineDateTimeConverter(new MockLogger());
            converter1.Configure(config1);

            var config2 = new ConverterConfiguration() { Id = ID_CONVERTER_2 };
            var converter2 = new DateTimeUtcConverter(new MockLogger());
            converter2.Configure(config2);

            var converters = new Dictionary<string, IConverter>();
            converters.Add(ID_CONVERTER_1, converter1);
            converters.Add(ID_CONVERTER_2, converter2);
            return converters;
        }
    }
}