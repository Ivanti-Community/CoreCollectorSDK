// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Collector.SDK.DataModel;

namespace Collector.SDK.Converters
{
    public class ArrayToDictionaryConverter : AbstractConverter
    {
        public override Dictionary<string, object> Convert(KeyValuePair<string, object> point, IEntityCollection data)
        {
            string key = "Id"; // default
            Properties.TryGetValue("ArrayKey", out key);

            Dictionary<string, object> result = new Dictionary<string, object>();
            var items = point.Value as IEnumerable;
            foreach (var item in items)
            {
                var type = item.GetType();
                var fields = type.GetMethods();
                foreach (var info in fields)
                {
                    if (info.Name.StartsWith("get_" + key))
                    {
                        var id = key + "_" + info.Invoke(item, new object[] { }) as string;
                        result.Add(id, item);
                        break;
                    }
                }

            }
            return result;
        }
    }
}
