using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collector.SDK.Tests.Mocks
{
	class MockNetwork
	{
		public string Address { get; set; }
		public List<MockBoundAdapter> BoundAdapters { get; set; }
	}
}
