// ***************************************************************
// Copyright 2019 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.Converters;
using Collector.SDK.DataModel;
using Collector.SDK.Logging;
using System;
using System.Collections.Generic;

namespace Collector.SDK.Samples.Converters
{
    public class SimpleDateTimeUtcConverter : AbstractConverter
    {
        private readonly ILogger _logger;

        public SimpleDateTimeUtcConverter(ILogger logger)
        {
            _logger = logger;
        }

        public override Dictionary<string, object> Convert(KeyValuePair<string, object> point, IEntityCollection data)
        {
            var result = new Dictionary<string, object>();
            try {
                DateTime utcDateTime = DateTime.Now.ToUniversalTime();
                if (point.Value is DateTime)
                {
                    utcDateTime = ((DateTime)point.Value).ToUniversalTime();
                }
                else
                {
                    utcDateTime = DateTime.Parse(point.Value as string).ToUniversalTime();
                }
                var names = ConvertLeftSide(point.Key);
                foreach (var name in names)
                {
                    result.Add(name, utcDateTime);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
                return result;
            }
            return result;
        }
    }
}
