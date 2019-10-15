using Collector.SDK.DataModel;
using System;

namespace Collector.SDK.Samples.DataModel
{
    public class LogEntry : IEntity
    {
        public DateTime DateTime { get; set; }
        public string Type { get; set; }
        public string Module { get; set; }
        public string Message { get; set; }
    }
}
