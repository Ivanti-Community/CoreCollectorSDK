// ***************************************************************
// Copyright 2019 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.Converters;
using Collector.SDK.DataModel;
using System;
using System.Collections.Generic;

namespace Collector.SDK.Samples.Converters
{
    public class SimpleDateTimeUtcConverter : AbstractConverter
    {
        public override Dictionary<string, object> Convert(KeyValuePair<string, object> point, IEntityCollection data)
        {
            var result = new Dictionary<string, object>();
            var names = ConvertLeftSide(point.Key);
            foreach (var name in names)
            {
                DateTime parsedDateTime;
                if (DateTime.TryParse((string)point.Value, out parsedDateTime))
                {
                    result.Add(name, parsedDateTime.ToUniversalTime());
                }
            }
            return result;
        }
    }
}
