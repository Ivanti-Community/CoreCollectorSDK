using Collector.SDK.Collectors;
using Collector.SDK.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Collector.SDK.Tests.Mocks
{
    public class MockInfiniteReader : AbstractReader
    {
        private bool _cancelled = false;

        public bool Cancelled => _cancelled;

        public MockInfiniteReader(ICollector collector) : base(collector)
        {
        }

        public override void Dispose()
        {
            // nada
        }

        public override Task Read(Dictionary<string, string> properties)
        {
            return Task.Factory.StartNew(() => {
                while (true)
                {
                    if (Token.IsCancellationRequested)
                    {
                        _cancelled = true;
                        break;
                    }
                    Thread.Sleep(100);
                }
            });
        }
    }
}
