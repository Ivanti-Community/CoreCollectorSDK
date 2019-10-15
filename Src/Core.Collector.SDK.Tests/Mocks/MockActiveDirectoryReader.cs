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
    public class MockActiveDirectoryReader : AbstractReader
    {
        private ICollector _collector;

        public MockActiveDirectoryReader(ICollector collector) : base(collector)
        {
            _collector = collector;
        }

        public override void Dispose()
        {
            // nada
        }

        public override async Task Read(Dictionary<string, string> properties)
        {
            var dataToConvert = new MockUser()
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane.doe@acme.com",
                LastLogin = "06-25-2018 09:03:45.123456"
            };

            IEntityCollection row = new EntityCollection();
            row.Entities.Add("AD User", dataToConvert);
            Data.Add(row);
            var context = new Dictionary<string, string>();
            // context.Add(CollectorConstants.STATE_READER_DONE, Id);
            await SignalHandler(context);

            await _collector.SignalEvent(new StateEvent()
            {
                SenderId = Id,
                State = CollectorConstants.STATE_READER_DONE
            });
        }
    }
}
