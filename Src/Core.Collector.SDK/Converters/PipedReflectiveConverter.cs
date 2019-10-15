// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System.Collections.Generic;
using Collector.SDK.DataModel;

namespace Collector.SDK.Converters
{
    public class PipedReflectiveConverter : AbstractConverter
    {
        public override Dictionary<string, object> Convert(KeyValuePair<string, object> point, IEntityCollection data)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            Dictionary<string, object> convertedValues = new Dictionary<string, object>();
	        if (point.Value == null)
				return result;
			var type = point.Value.GetType();
            var fields = type.GetMethods();
            foreach (var info in fields)
            {
                if (info.Name.StartsWith("get_"))
                {
                    var key = info.Name.Substring("get_".Length);
                    var converter = FindConverter(key);
                    if (converter != null)
                    {
                        var value = info.Invoke(point.Value, new object[] { });
                        var convertedDataPoints = converter.Convert(new KeyValuePair<string, object>(key, value), data);
                        foreach (var convertedValue in convertedDataPoints)
                        {
                            convertedValues.Add(convertedValue.Key, convertedValue.Value);
                        }
                    }
                }
            }
            
            var convertedKeys = ConvertLeftSide(point.Key);
            foreach (var convertedKey in convertedKeys) {
				result.Add(convertedKey, convertedValues);
			}
            return result;
        }
        private IConverter FindConverter(string key)
        {
            foreach (var converter in PipedConverters)
            {
                var leftSideMap = converter.LeftSideMap;
                if (leftSideMap != null)
                {
                    if (leftSideMap.ContainsKey(key))
                    {
                        return converter;
                    }
                }
            }
            return null;
        }
    }
}
