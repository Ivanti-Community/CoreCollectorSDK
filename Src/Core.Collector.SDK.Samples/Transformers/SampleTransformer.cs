// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System.Collections.Generic;
using System.Threading.Tasks;
using Collector.SDK.Collectors;
using Collector.SDK.DataModel;
using Collector.SDK.Logging;
using Collector.SDK.Transformers;

namespace Collector.SDK.Samples.Transformers
{
    public class SampleTransformer : AbstractTransformer
    {
        private readonly ILogger _logger;
        private ICollector _collector;

        public SampleTransformer(ILogger logger, ICollector collector) : base(collector)
        {
            _logger = logger;
            _collector = collector;
        }

        public override async Task Transform(string senderId, List<IEntityCollection> data, Dictionary<string, string> context)
        {
            foreach (var mapper in Mappers)
            {
                _logger.Info("Handling data from sender {0}", senderId);

                Data = mapper.Map(data);
                await SignalPublisher(context);
            }
        }
    }
}
