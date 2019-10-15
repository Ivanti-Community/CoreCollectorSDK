// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.Collectors;
using Collector.SDK.Logging;
using Collector.SDK.Publishers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Collector.SDK.Tests.Mocks
{
    class MockPublisher : AbstractPublisher
    {
        private ILogger _logger;
        private ICollector _collector;
        private bool _invoked = false;
        private string _senderId;
        private List<object> _data;
        private Dictionary<string, string> _context;

        public bool Invoked => _invoked;
        public string SenderId => _senderId;
        public List<object> Data => _data;
        public Dictionary<string, string> Context => _context;


        public MockPublisher(ILogger logger, ICollector collector) : base(collector)
        {
            _logger = logger;
            _collector = collector;
        }

        public override async Task Publish(string senderId, List<object> data, Dictionary<string, string> context)
        {
            _senderId = senderId;
            _data = data;
            _invoked = true;
            _context = context;
            Console.WriteLine("Sender : {0}, Data {1}", senderId, data);

            await _collector.SignalEvent(new StateEvent()
            {
                SenderId = Id,
                State = CollectorConstants.STATE_PUBLISHER_DONE
            });
        }
    }
}
