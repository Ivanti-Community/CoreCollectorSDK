// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.Transformers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Collector.SDK.Tests.Mocks
{
    public class MockTransformationHandler : ITransformedDataHandler
    {
        private bool _invoked = false;
        private string _senderId;
        private List<object> _data;
        private Dictionary<string, string> _context;

        public bool Invoked => _invoked;
        public string SenderId => _senderId;
        public List<object> Data => _data;
        public Dictionary<string, string> Context => _context;

        public Task PublishData(string senderId, List<object> data, Dictionary<string, string> context)
        {
            return Task.Run(() => 
            {
                _senderId = senderId;
                _data = data;
                _invoked = true;
                _context = context;
            });
        }
    }
}
