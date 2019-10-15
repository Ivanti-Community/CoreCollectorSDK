// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System.Collections.Generic;
using System.Threading.Tasks;
using Collector.SDK.Collectors;
using Collector.SDK.Configuration;

namespace Collector.SDK.Publishers
{
    public abstract class AbstractPublisher : IPublisher
    {
        private ICollector _collector;
        private string _id;
        private EndPointConfiguration _config;
        private bool _done = false;
        public string Id => _id;
        public EndPointConfiguration EndPointConfig => _config;
        public bool Done => _done;
        public AbstractPublisher(ICollector collector)
        {
            _collector = collector;
        }
        public virtual void Configure(string id, EndPointConfiguration config)
        {
            _id = id;
            _config = config;
        }

        public async Task PublishData(string senderId, List<object> data, Dictionary<string, string> context)
        {
            if (data != null && data.Count != 0)
            {
                await Publish(senderId, data, context);
            }
            // Only signal tha we are done when the reader is done.
            if (context.ContainsKey(CollectorConstants.KEY_STATE)
                && context[CollectorConstants.KEY_STATE] == CollectorConstants.STATE_READER_DONE)
            {
                _done = true;
                await _collector.SignalEvent(new StateEvent()
                {
                    SenderId = Id,
                    State = CollectorConstants.STATE_PUBLISHER_DONE,
                    ExtraInfo = context
                });
            }
        }

        public abstract Task Publish(string senderId, List<object> data, Dictionary<string, string> context);
    }
}
