// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System.Collections.Generic;
using Collector.SDK.DataModel;

namespace Collector.SDK.Converters
{
    public class ReflectiveConverter : AbstractConverter
    {
        public override Dictionary<string, object> Convert(KeyValuePair<string, object> point, IEntityCollection data)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            var type = point.Value.GetType();
            var fields = type.GetMethods();
            foreach (var info in fields)
            {
                if (info.Name.StartsWith("get_"))
                {
                    var key = info.Name.Substring("get_".Length);
                    result.Add(key, info.Invoke(point.Value, new object[] { }));
                }
            }
            return result;
        }
    }
}
