using Collector.SDK.Converters;
using Collector.SDK.DataModel;
using Collector.SDK.Mappers;
using Collector.SDK.Samples.Transformers.DataModel;
using System.Collections.Generic;

namespace Collector.SDK.Samples.Converters
{
    public class NetworkConverter : AbstractConverter
    {
        private object ConvertRightSide(object value, IEntityCollection data)
        {
            var networkCard = new NetworkCard();
            networkCard.MacAddress = value as string;

            var key = "";
            Properties.TryGetValue("IP Address", out key);
            object ipAddress;
            data.Entities.TryGetValue(key, out ipAddress);
            networkCard.IPAddress = ipAddress as string;
            return networkCard;
        }
        public override Dictionary<string, object> Convert(KeyValuePair<string, object> point, IEntityCollection data)
        {
            var result = new Dictionary<string, object>();
            var names = ConvertLeftSide(point.Key);
            foreach (var name in names)
            {
                result.Add(name, ConvertRightSide(point.Value, data));
            }
            return result;
        }
    }
}
