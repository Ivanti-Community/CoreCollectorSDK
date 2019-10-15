// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.Converters;
using Collector.SDK.DataModel;
using Collector.SDK.Mappers;
using System.Collections.Generic;

namespace Collector.SDK.Converters
{
    public class AllUpperCaseConverter : AbstractConverter
    {
        public override Dictionary<string, object> Convert(KeyValuePair<string, object> dataPoint, IEntityCollection row)
        {
            var result = new Dictionary<string, object>();
            var names = ConvertLeftSide(dataPoint.Key);
            foreach (var name in names)
            {
                result.Add(name, (dataPoint.Value as string).ToUpper());
            }
            return result;
        }
    }
}
