// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using Collector.SDK.DataModel;
using Collector.SDK.Logging;

namespace Collector.SDK.Converters
{
    public class DateTimeUtcConverter : AbstractConverter
    {
        private readonly ILogger _logger;
        public DateTimeUtcConverter(ILogger logger)
        {
            _logger = logger;
        }

        private object ConvertRightSide(object value)
        {
	        if (value == null)
				return value;

	        if (!(value is DateTime) && string.IsNullOrEmpty(value as string))
		        return value;

	        if (value is DateTime)
	        {
		        return ToUniversalTime((DateTime)value);
	        }

			Properties.TryGetValue("DateTimeFormat", out var format); 
            DateTime newDate;

            if (format != null)
            {

                if (TryParseExact((string)value, format, null, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault, out newDate))
                {
                    return ToUniversalTime(newDate);
                }
				if (DateTime.TryParseExact((string)value, format + "K", null, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault, out newDate))
				{
					return ToUniversalTime(newDate);
	            }
				// ISO 8601
				if (DateTime.TryParseExact((string)value, "o", null, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault, out newDate))
				{
					return ToUniversalTime(newDate);
				}
            }
            if (DateTime.TryParse((string)value, out newDate))
            {
                return ToUniversalTime(newDate);
            }
            _logger.Error("Unable to parse date time : {0}", value);
            return value;
        }

	    private bool TryParseExact(string s, string format, IFormatProvider provider, DateTimeStyles style, out DateTime newDate)
	    {
		    if (DateTime.TryParseExact(s, format, provider, style, out newDate))
		    {
			    return true;
		    }
		    _logger.Debug("Unable to parse date time using format {0} with value {1}", format, s);
			return false;
	    }

        private string ToUniversalTime(DateTime value)
        {
            TimeZoneInfo timezoneFromProp;

            switch (value.Kind)
            {
                case DateTimeKind.Local:
                    value = value.ToUniversalTime();
                    break;
                case DateTimeKind.Unspecified:
                    timezoneFromProp = GetTimezone();
                    value = TimeZoneInfo.ConvertTimeToUtc(value, timezoneFromProp);
                    break;
                case DateTimeKind.Utc:
                    break;
            }
			return value.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK");
        }

	    private TimeZoneInfo GetTimezone()
	    {
            string timezoneId;
            Properties.TryGetValue("TimeZone", out timezoneId);
            var currentZone = TimeZoneInfo.Utc;

            if (timezoneId != null)
            {
                currentZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            }
            return currentZone;
	    }

        public override Dictionary<string, object> Convert(KeyValuePair<string, object> point, IEntityCollection row)
        {
            var result = new Dictionary<string, object>();
            var names = ConvertLeftSide(point.Key);
            foreach (var name in names)
            {
                result.Add(name, ConvertRightSide(point.Value));
            }
            return result;
        }
    }
}