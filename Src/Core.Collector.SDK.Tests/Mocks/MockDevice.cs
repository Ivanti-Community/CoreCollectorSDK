using System.Collections.Generic;
using Collector.SDK.DataModel;

namespace Collector.SDK.Tests.Mocks
{
	class MockDevice : IEntity
	{
		public string DeviceId { get; set; }
		public List<MockSoftware> Software { get; set; }
		public MockNetwork Network { get; set; }
	}
}
