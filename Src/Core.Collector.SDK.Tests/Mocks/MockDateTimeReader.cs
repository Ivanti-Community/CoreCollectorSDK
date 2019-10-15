// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System.Collections.Generic;
using System.Threading.Tasks;
using Collector.SDK.Collectors;
using Collector.SDK.DataModel;
using Collector.SDK.Readers;

namespace Collector.SDK.Tests.Mocks
{
    public class MockDateTimeReader : AbstractReader
    {
        private ICollector _collector;

        public MockDateTimeReader(ICollector collector) : base(collector)
        {
            _collector = collector;
        }
        public override void Dispose()
        {
            // Nada
        }

        public override async Task Read(Dictionary<string, string> properties)
        {
            var dateToConvert = "01/01/2018";
            var timeToConvert = "01:00:55.000";

            IEntityCollection row = new EntityCollection();
            row.Entities.Add("Time", timeToConvert);
            row.Entities.Add("Date", dateToConvert);
            Data.Add(row);
            var context = new Dictionary<string, string>();
            await SignalHandler(context);

            await _collector.SignalEvent(new StateEvent()
            {
                SenderId = Id,
                State = CollectorConstants.STATE_READER_DONE
            });
        }
    }
}
