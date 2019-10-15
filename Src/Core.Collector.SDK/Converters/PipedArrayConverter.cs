// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Collector.SDK.DataModel;

namespace Collector.SDK.Converters
{
    public class PipedArrayConverter : AbstractConverter
    {
        /// <summary>
        /// Converters an array of objects by using reflection
        /// </summary>
        /// <param name="point"></param>
        /// <param name="data"></param>
        /// <returns>
        ///     Returns the converted array items as a dictionary of dictionaries.  
        ///     The dictionary key is defined as a Prefix_Identifier.  
        ///     The prefix is configured in the collector config sourec target mapper Properties["ArrayKey"]
        /// </returns>
        public override Dictionary<string, object> Convert(KeyValuePair<string, object> point, IEntityCollection data)
        {
	        if (!Properties.TryGetValue("ArrayKey", out var valueArrayKey))
            {
                throw new ArgumentException("Missing ArrayKey Property");
            }
            var arrayKey = valueArrayKey.Split(',');

            var dictResult = new Dictionary<string, object>();
            var result = new Dictionary<string, object>();

            var array = point.Value as IEnumerable;
            var id = new KeyValuePair<string, object>();

	        if (array == null)
	        {
		        return result;
	        }

            foreach (var item in array)
            {
                var convertedData = new Dictionary<string, object>();
	            if (item == null)
	            {
		            continue;
	            }
				var type = item.GetType();
	            
                var fields = type.GetMethods();
                foreach (var info in fields)
                {
                    if (info.Name.StartsWith("get_"))
                    {
                        var itemName = info.Name.Substring("get_".Length);
                        var converter = FindConverter(itemName);
                        if (converter != null)
                        {
                            var field = new KeyValuePair<string, object>(itemName, info.Invoke(item, new object[] { }));
							if (null == field.Value)
								continue;
							var convertedField = converter.Convert(field, data);

                            // Check if this data point is the array id.
                            foreach (var key in arrayKey)
                            {
                                if (itemName.Equals(key))
                                {
                                    id = new KeyValuePair<string, object>(itemName, field.Value);
                                    break;
                                }
                            }

                            foreach (var convertedKey in convertedField.Keys)
                            {
                                convertedData[convertedKey] = convertedField[convertedKey];
                            }
                        }
                    }
                }
                if (id.Key != null)
                {
                    dictResult[string.Format(CultureInfo.InvariantCulture, "{0}_{1}", id.Key, id.Value)] = convertedData;
                }
            }

            var names = ConvertLeftSide(point.Key);

            //Copy fields from a dictionary to an array
            foreach (var name in names)
            {
                var fieldsAsArray = new List<object>();
                foreach (var currentKey in dictResult.Keys)
                {
                    fieldsAsArray.Add(dictResult[currentKey]);
                }

                result[name + "_pkAttrName"] = id.Key;
                result[name] = fieldsAsArray;
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
