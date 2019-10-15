// ***************************************************************
// Copyright 2019 Ivanti Inc. All rights reserved.
// ***************************************************************
using System;
using System.Collections.Generic;
using Collector.SDK.DataModel;

namespace Collector.SDK.Converters
{
    public class DateTimeConverter : AbstractConverter
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
                    result.Add(name, parsedDateTime);
                }
            }
            return result;
        }
    }
}
