// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System.Collections.Generic;
using System.Threading.Tasks;
using Collector.SDK.Collectors;
using Collector.SDK.DataModel;
using Collector.SDK.Logging;
using Collector.SDK.Transformers;

namespace Collector.SDK.Tests.Mocks
{
    public class MockTransformer : AbstractTransformer
    {
        private ILogger _logger;
        private ICollector _collector;

        public MockTransformer(ILogger logger, ICollector collector) : base(collector)
        {
            _logger = logger;
            _collector = collector;
        }
        public override async Task Transform(string senderId, List<IEntityCollection> data, Dictionary<string, string> context)
        {
            foreach (var mapper in Mappers)
            {
                Data = mapper.Map(data);
                await SignalPublisher(context);
            }
            await _collector.SignalEvent(new StateEvent()
            {
                SenderId = Id,
                State = CollectorConstants.STATE_TRANSFORMER_DONE
            });
        }
    }
}
