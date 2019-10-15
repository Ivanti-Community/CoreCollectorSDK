// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.Collectors;
using Collector.SDK.DataModel;
using Collector.SDK.Readers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Collector.SDK.Tests.Mocks
{
    public class MockReader : AbstractReader
    {
        private ICollector _collector;
        public MockReader(ICollector collector) : base(collector)
        {
            _collector = collector;
        }

        public override void Dispose()
        {
            // nada
        }

        public override async Task Read(Dictionary<string, string> properties)
        {
            IEntityCollection row = new EntityCollection();
            row.Entities.Add("foo", "bar");
            Data.Add(row);

            await SignalHandler(new Dictionary<string, string>());

            await _collector.SignalEvent(new StateEvent()
            {
                SenderId = Id,
                State = CollectorConstants.STATE_READER_DONE
            });
        }
    }
}
