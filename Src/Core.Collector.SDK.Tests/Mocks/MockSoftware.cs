using Collector.SDK.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collector.SDK.Tests.Mocks
{
    public class MockSoftware : IEntity
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime DateTime { get; set; }
        public string DateTimeUTC { get; set; }
    }
}
