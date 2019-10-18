// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using Collector.SDK.Configuration;
using Collector.SDK.Converters;
using Collector.SDK.DataModel;
using Collector.SDK.Tests.Mocks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Collector.SDK.Tests
{
    [TestClass]
    public class ConverterTests
    {
        [TestMethod]
        [Owner("roy.morris@ivanti.com")]
		[TestCategory("Unit")]
        public void NameOnlyConverter_Convert_Success()
        {
            var config = new ConverterConfiguration()
            {
                LeftSideMap = new Dictionary<string, List<string>>() { { "ABC", new List<string>() { "XYZ" } } },
                Properties = new Dictionary<string, string>()
            };
            var converter = new NameOnlyConverter();
            converter.Configure(config);

            var dataPoint = new KeyValuePair<string, object>("ABC", "123");
            var dataRow = new EntityCollection();
            dataRow.Entities.Add(dataPoint.Key, dataPoint.Value);

            var convertedPoint = converter.Convert(dataPoint, dataRow);
            convertedPoint.Should().NotBeNull();
            convertedPoint["XYZ"].Should().Equals("123");
        }

		[TestMethod]
        [Owner("roy.morris@ivanti.com")]
		[TestCategory("Unit")]
        public void NoOpConverter_Convert_Success()
        {
            var config = new ConverterConfiguration()
            {
                LeftSideMap = new Dictionary<string, List<string>>() { { "ABC", new List<string>() { "XYZ" } } },
                Properties = new Dictionary<string, string>()
            };
            var converter = new NoOpConverter();
            converter.Configure(config);

            var dataPoint = new KeyValuePair<string, object>("ABC", "123");
            var dataRow = new EntityCollection();
            dataRow.Entities.Add(dataPoint.Key, dataPoint.Value);

            var convertedPoint = converter.Convert(dataPoint, dataRow);
            convertedPoint.Should().NotBeNull();
            convertedPoint["XYZ"].Should().Equals("123");
        }

		[TestMethod]
        [Owner("roy.morris@ivanti.com")]
		[TestCategory("Unit")]
        public void NoOpConverter_NoLeftSideMap_Convert_Success()
        {
            var config = new ConverterConfiguration()
            {
                Properties = new Dictionary<string, string>()
            };
            var converter = new NoOpConverter();
            converter.Configure(config);

            var dataPoint = new KeyValuePair<string, object>("ABC", "123");
            var dataRow = new EntityCollection();
            dataRow.Entities.Add(dataPoint.Key, dataPoint.Value);

            var convertedPoint = converter.Convert(dataPoint, dataRow);
            convertedPoint.Should().NotBeNull();
            convertedPoint["ABC"].Should().Equals("123");
        }

		[TestMethod]
        [Owner("roy.morris@ivanti.com")]
		[TestCategory("Unit")]
        public void UpperCaseConverter_Convert_Success()
        {
            var config = new ConverterConfiguration()
            {
                LeftSideMap = new Dictionary<string, List<string>>() { { "ABC", new List<string>() { "XYZ" } } },
                Properties = new Dictionary<string, string>()
            };
            var converter = new AllUpperCaseConverter();
            converter.Configure(config);

            var dataPoint = new KeyValuePair<string, object>("ABC", "foobar");
            var dataRow = new EntityCollection();
            dataRow.Entities.Add(dataPoint.Key, dataPoint.Value);

            var convertedPoint = converter.Convert(dataPoint, dataRow);
            convertedPoint.Should().NotBeNull();
            convertedPoint["XYZ"].Should().Equals("FOOBAR");
        }

		[TestMethod]
        [Owner("roy.morris@ivanti.com")]
		[TestCategory("Unit")]
        public void CombineDateTimeConverter_Convert_Success()
        {
            var config = new ConverterConfiguration()
            {
                LeftSideMap = new Dictionary<string, List<string>>() { { "Date", new List<string>() { "DateTime" } } },
                Properties = new Dictionary<string, string>() { { "DateTimeFormat", "yyyy-MM-dd hh:mm:ss.fff" }}
            };
            var converter = new CombineDateTimeConverter(new MockLogger());
            converter.Configure(config);

            var dataPoint = new KeyValuePair<string, object>("Date", "2017-01-01");
            var dataRow = new EntityCollection();
            dataRow.Entities.Add("Time", "22:11:00.000");
            dataRow.Entities.Add(dataPoint.Key, dataPoint.Value);

            var convertedPoint = converter.Convert(dataPoint, dataRow);
            convertedPoint.Should().NotBeNull();
            convertedPoint["DateTime"].ToString().Should().Equals("1/2/2017 6:11:00 AM"); // UTC
        }

		[TestMethod]
        [Owner("roy.morris@ivanti.com")]
		[TestCategory("Unit")]
        public void DateTimeUtcConverter_Convert_Success()
        {
            var config = new ConverterConfiguration()
            {
                LeftSideMap = new Dictionary<string, List<string>>() { { "DateTime", new List<string>() { "DateTime" } } },
                Properties = new Dictionary<string, string>()
            };
            var converter = new DateTimeUtcConverter(new MockLogger());
            converter.Configure(config);

            var dataPoint = new KeyValuePair<string, object>("DateTime", "2017-01-01 22:11:00.000");
            var dataRow = new EntityCollection();
            dataRow.Entities.Add(dataPoint.Key, dataPoint.Value);

            var convertedPoint = converter.Convert(dataPoint, dataRow);
            convertedPoint.Should().NotBeNull();
            Assert.AreEqual("2017-01-01T22:11:00.000Z", convertedPoint["DateTime"].ToString());
        }

		[TestMethod]
        [Owner("jason.kingsford@ivanti.com")]
        [TestCategory("Unit")]
        public void DateTimeUtcConverter_ConvertWithOffset_Success()
        {
            var config = new ConverterConfiguration()
            {
                LeftSideMap = new Dictionary<string, List<string>>() { { "DateTime", new List<string>() { "DateTime" } } },
                Properties = new Dictionary<string, string>() { { "TimeZone", "Mountain Standard Time" } }
            };
            var converter = new DateTimeUtcConverter(new MockLogger());
            converter.Configure(config);

            var dataPoint = new KeyValuePair<string, object>("DateTime", "2017-01-01 22:11:00.000");
            var dataRow = new EntityCollection();
            dataRow.Entities.Add(dataPoint.Key, dataPoint.Value);

            var convertedPoint = converter.Convert(dataPoint, dataRow);
            convertedPoint.Should().NotBeNull();
            Assert.AreEqual("2017-01-02T05:11:00.000Z", convertedPoint["DateTime"].ToString()); // UTC
        }
		
        [TestMethod]
        [Owner("jason.kingsford@ivanti.com")]
        [TestCategory("Unit")]
        public void DateTimeUtcConverter_ConvertTimeOnly_Success()
        {
            var config = new ConverterConfiguration()
            {
                LeftSideMap = new Dictionary<string, List<string>>() { { "DateTime", new List<string>() { "DateTime" } } },
                Properties = new Dictionary<string, string>() { { "TimeZone", "Mountain Standard Time" }, { "DateTimeFormat", "HH:mm"} }
            };
            var converter = new DateTimeUtcConverter(new MockLogger());
            converter.Configure(config);

            var dataPoint = new KeyValuePair<string, object>("DateTime", "09:01");
            var dataRow = new EntityCollection();
            dataRow.Entities.Add(dataPoint.Key, dataPoint.Value);

            var convertedPoint = converter.Convert(dataPoint, dataRow);
            convertedPoint.Should().NotBeNull();
            var udts = (string)convertedPoint["DateTime"];
            Assert.AreEqual("16:01:00.000Z", udts.Split('T')[1]); // UTC
        }

        [TestMethod]
        [Owner("jason.kingsford@ivanti.com")]
        [TestCategory("Unit")]
        public void DateTimeUtcConverter_ConvertWithInvalidFormat_Success()
        {
            var config = new ConverterConfiguration()
            {
                LeftSideMap = new Dictionary<string, List<string>>() { { "DateTime", new List<string>() { "DateTime" } } },
                Properties = new Dictionary<string, string>() { { "TimeZone", "Mountain Standard Time" }, { "DateTimeFormat", "HH:mm" } }
            };
            var converter = new DateTimeUtcConverter(new MockLogger());
            converter.Configure(config);

            var dataPoint = new KeyValuePair<string, object>("DateTime", "2017-01-01 22:11:00.000");
            var dataRow = new EntityCollection();
            dataRow.Entities.Add(dataPoint.Key, dataPoint.Value);

            var convertedPoint = converter.Convert(dataPoint, dataRow);
            convertedPoint.Should().NotBeNull();
            Assert.AreEqual("2017-01-02T05:11:00.000Z", convertedPoint["DateTime"].ToString()); // UTC
        }

        [TestMethod]
        [Owner("jason.kingsford@ivanti.com")]
        [TestCategory("Unit")]
        public void DateTimeUtcConverter_ConvertFails_ReturnsOrgString()
        {
            var config = new ConverterConfiguration()
            {
                LeftSideMap = new Dictionary<string, List<string>>() { { "DateTime", new List<string>() { "DateTime" } } },
                Properties = new Dictionary<string, string>() { { "TimeZone", "50" }, { "DateTimeFormat", "HH:mm" } }
            };
            var converter = new DateTimeUtcConverter(new MockLogger());
            converter.Configure(config);

            var dataPoint = new KeyValuePair<string, object>("DateTime", "This is crap text");
            var dataRow = new EntityCollection();
            dataRow.Entities.Add(dataPoint.Key, dataPoint.Value);

            var convertedPoint = converter.Convert(dataPoint, dataRow);
            convertedPoint.Should().NotBeNull();
            convertedPoint.Count.Should().Be(0);
        }

        [TestMethod]
        [Owner("jason.kingsford@ivanti.com")]
        [TestCategory("Unit")]
        public void DateTimeUtcConverter_ExactFormatCases()
        {
	        const string timeZoneId = "50";
	        const string dateTimeFormat = "HH:mm zzz";
	        const string time = "09:01+01:00";

			var config = new ConverterConfiguration()
            {
                LeftSideMap = new Dictionary<string, List<string>>() { { "DateTime", new List<string>() { "DateTime" } } },
				Properties = new Dictionary<string, string>() { { "TimeZone", timeZoneId }, { "DateTimeFormat", dateTimeFormat } }
			};
            var converter = new DateTimeUtcConverter(new MockLogger());
            converter.Configure(config);

			var dataPoint = new KeyValuePair<string, object>("DateTime", time);
			var dataRow = new EntityCollection();
            dataRow.Entities.Add(dataPoint.Key, dataPoint.Value);

            var convertedPoint = converter.Convert(dataPoint, dataRow);
			var dateTime = DateTime.ParseExact(time, dateTimeFormat, null, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault);
			dateTime = dateTime.ToUniversalTime();
			var expectedValue = dateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK");

			convertedPoint.Should().NotBeNull();
			Assert.AreEqual(expectedValue, convertedPoint["DateTime"].ToString());
		}

        [TestMethod]
        [Owner("jason.kingsford@ivanti.com")]
        [TestCategory("Unit")]
        public void DateTimeUtcConverter_ExactFormatCases1()
        {
            var config = new ConverterConfiguration()
            {
                LeftSideMap = new Dictionary<string, List<string>>() { { "DateTime", new List<string>() { "DateTime" } } },
                Properties = new Dictionary<string, string>() { { "TimeZone", "300" }, { "DateTimeFormat", "MM/dd/yyyy HH:mm:ss" } }
            };
            var converter = new DateTimeUtcConverter(new MockLogger());
            converter.Configure(config);

            var dataPoint = new KeyValuePair<string, object>("DateTime", "05/24/2017 11:02:33+01:00");
            var dataRow = new EntityCollection();
            dataRow.Entities.Add(dataPoint.Key, dataPoint.Value);

            var convertedPoint = converter.Convert(dataPoint, dataRow);
            convertedPoint.Should().NotBeNull();
            Assert.AreEqual("2017-05-24T10:02:33.000Z", convertedPoint["DateTime"].ToString());
        }

	    [TestMethod]
	    [Owner("jason.kingsford@ivanti.com")]
	    [TestCategory("Unit")]
	    public void DateTimeUtcConverter_HandleIso8601()
	    {
		    var config = new ConverterConfiguration()
		    {
			    LeftSideMap = new Dictionary<string, List<string>>() { { "DateTime", new List<string>() { "DateTime" } } },
			    Properties = new Dictionary<string, string>() { { "TimeZone", "Eastern Standard Time" }, { "DateTimeFormat", "yyyy-MM-dd HH:mm:ss" } }
		    };
		    var converter = new DateTimeUtcConverter(new MockLogger());
		    converter.Configure(config);

		    var dataPoint = new KeyValuePair<string, object>("DateTime", "2017-05-24T11:02:33.500+01:00");
		    var dataRow = new EntityCollection();
		    dataRow.Entities.Add(dataPoint.Key, dataPoint.Value);

		    var convertedPoint = converter.Convert(dataPoint, dataRow);
		    convertedPoint.Should().NotBeNull();
		    Assert.AreEqual("2017-05-24T10:02:33.500Z", convertedPoint["DateTime"].ToString());
	    }

	    [TestMethod]
	    [Owner("jason.kingsford@ivanti.com")]
	    [TestCategory("Unit")]
	    public void DateTimeUtcConverter_HandleMoreIso8601()
	    {
		    var config = new ConverterConfiguration()
		    {
			    LeftSideMap = new Dictionary<string, List<string>>() { { "DateTime", new List<string>() { "DateTime" } } },
			    Properties = new Dictionary<string, string>() { { "TimeZone", "Eastern Standard Time" }, { "DateTimeFormat", "yyyy-MM-dd HH:mm:ss" } }
		    };
		    var converter = new DateTimeUtcConverter(new MockLogger());
		    converter.Configure(config);

		    var dataPoint = new KeyValuePair<string, object>("DateTime", "2018-08-20T13:44:55.6000000-07:00");
		    var dataRow = new EntityCollection();
		    dataRow.Entities.Add(dataPoint.Key, dataPoint.Value);

		    var convertedPoint = converter.Convert(dataPoint, dataRow);
		    convertedPoint.Should().NotBeNull();
		    Assert.AreEqual("2018-08-20T20:44:55.600Z", convertedPoint["DateTime"].ToString());
	    }

		[TestMethod]
        [Owner("jason.kingsford@ivanti.com")]
        [TestCategory("Unit")]
        public void DateTimeUtcConverter_TestCleaningData()
        {
            var config = new ConverterConfiguration()
            {
                LeftSideMap = new Dictionary<string, List<string>>() { { "DateTime", new List<string>() { "DateTime" } } },
                Properties = new Dictionary<string, string>() { { "TimeZone", "Eastern Standard Time" }, { "DateTimeFormat", "MM/dd/yyyy hh:mm:ss" } }
            };
            var converter = new DateTimeUtcConverter(new MockLogger());
            converter.Configure(config);

            var dataPoint = new KeyValuePair<string, object>("DateTime", " 05/04/2017      11:02:33+01:00 ");
            var dataRow = new EntityCollection();
            dataRow.Entities.Add(dataPoint.Key, dataPoint.Value);

            var convertedPoint = converter.Convert(dataPoint, dataRow);
            convertedPoint.Should().NotBeNull();
            Assert.AreEqual("2017-05-04T10:02:33.000Z", convertedPoint["DateTime"].ToString());
        }
    }
}