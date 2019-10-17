using Collector.SDK.Converters;
using Collector.SDK.DataModel;
using System.Collections.Generic;

namespace Collector.SDK.Samples.Converters
{
    public class LogMessageConverter : AbstractConverter
    {
        public override Dictionary<string, object> Convert(KeyValuePair<string, object> point, IEntityCollection data)
        {
            var result = new Dictionary<string, object>();
            var message = point.Value as string;
            var concat = false;
            foreach (var entity in data.Entities)
            {
                if (concat)
                {
                    message = string.Format("{0} {1}", message, entity.Value);
                }
                if (entity.Key.Equals(point.Key))
                {
                    concat = true;
                }
            }
            var names = ConvertLeftSide(point.Key);
            foreach (var name in names)
            {
                result.Add(name, message);
            }
            return result;
        }
    }
}
