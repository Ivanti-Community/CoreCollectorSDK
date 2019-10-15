// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System.Collections.Generic;
using Collector.SDK.DataModel;

namespace Collector.SDK.Converters
{
    public class NameOnlyConverter : AbstractConverter
    {
        public override Dictionary<string, object> Convert(KeyValuePair<string, object> point, IEntityCollection row)
        {
            var result = new Dictionary<string, object>();
            var names = ConvertLeftSide(point.Key);
            foreach (var name in names)
            {
                result.Add(name, point.Value);
            }
            return result;
        }
    }
}
