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
    public class CombineDateTimeConverter : AbstractConverter
    {
        private readonly ILogger _logger;
        public CombineDateTimeConverter(ILogger logger)
        {
            _logger = logger;
        }
        private DateTime ConvertRightSide(string value)
        {
            try
            {
                var format = "yyyy-MM-dd HH:mm:ss.fff";
                Properties.TryGetValue("DateTimeFormat", out format);
                if (!string.IsNullOrEmpty(value as string))
                {
                    var newDate = DateTime.Parse((string)value);
                    return newDate.ToUniversalTime();
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
            return DateTime.UtcNow;
        }

        public override Dictionary<string, object> Convert(KeyValuePair<string, object> point, IEntityCollection data)
        {
            var result = new Dictionary<string, object>();

            var sdate = point.Value as string;
            var timeKey = "Time";
            if (Properties.ContainsKey("TimeKey"))
            {
                timeKey = Properties["TimeKey"];
            }
            var stime = data.Entities[timeKey] as string;
            if (!string.IsNullOrEmpty(sdate) && !string.IsNullOrEmpty(stime))
            {
                stime = stime.Replace(",", ".");
                var dateTime = ConvertRightSide(string.Format(CultureInfo.InvariantCulture, "{0} {1}", sdate, stime));

                var names = ConvertLeftSide(point.Key);
                foreach (var name in names)
                {
                    result.Add(name, dateTime);
                }
            }
            return result;
        }
    }
}
