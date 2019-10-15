// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.Collectors;
using Collector.SDK.Logging;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Collector.SDK.Tests.Mocks
{
    public class MockCollector : AbstractCollector
    {
        private readonly ILogger _logger;

        public MockCollector(ILogger logger) : base(logger)
        {
            _logger = logger;
        }

        public override Task HandleEvent(StateEvent state)
        {
            return Task.Run(() =>
            {
                _logger.Info("Handling state change event {0}", state.State);
            });
        }

        public override Task Run()
        {
            return Task.Run(() =>
            {
                List<Task> tasks = new List<Task>();
                foreach (var readerId in ReaderIds)
                {
                    var properties = new Dictionary<string, string>();

                    var stack = CreateStack(readerId);

                    _logger.Info(string.Format(CultureInfo.InvariantCulture, "Executing reader {0}", stack.Reader.Id));
                    tasks.Add(stack.Run(properties));
                }
                Task.WaitAll(tasks.ToArray());
            });
        }
    }
}
