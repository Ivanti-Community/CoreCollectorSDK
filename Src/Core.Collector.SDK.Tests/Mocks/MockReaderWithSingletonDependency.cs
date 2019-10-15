using System.Collections.Generic;
using System.Threading.Tasks;

using Collector.SDK.Collectors;
using Collector.SDK.Readers;

namespace Collector.SDK.Tests.Mocks
{
    public class MockReaderWithSingletonDependency : AbstractReader
    {
        private MockDisposableSingleton _singletonDependency;

        public MockReaderWithSingletonDependency(ICollector collector, MockDisposableSingleton singletonDependency) : base(collector)
        {
            _singletonDependency = singletonDependency;
        }

        public override void Dispose()
        {
        }

        public override Task Read(Dictionary<string, string> properties)
        {
            return Task.CompletedTask;
        }
    }
}
