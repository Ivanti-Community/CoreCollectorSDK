// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.DataModel;
using Collector.SDK.Readers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Collector.SDK.Tests.Mocks
{
    public class MockHandler : IDataHandler
    {
        private bool _invoked = false;
        private List<IEntityCollection> _data;
        private Dictionary<string, string> _context;

        public bool Invoked => _invoked;
        public List<IEntityCollection> Data => _data;

        public Dictionary<string, string> Context => _context;

        public Task HandleData(string senderId, List<IEntityCollection> data, Dictionary<string, string> context)
        {
            return Task.Run(() => 
            {
                _data = data;
                _invoked = true;
                _context = context;
            });
        }
    }
}
