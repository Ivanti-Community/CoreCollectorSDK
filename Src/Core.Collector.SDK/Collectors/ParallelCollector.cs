// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.Logging;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Collector.SDK.Collectors
{
    public class ParallelCollector : AbstractCollector
    {
        private readonly ILogger _logger;
 
        public ParallelCollector(ILogger logger) : base(logger)
        {
            _logger = logger;
        }
        public override Task HandleEvent(StateEvent state)
        {
            return Task.Run(() => 
            {
                _logger.Info("Reader {0} is {1}", state.SenderId, state.State);
            });
        }
        /// <summary>
        /// Run all the connectors in parallel.
        /// </summary>
        public override Task Run()
        {
            return Task.Run(() =>
            {
                foreach (var readerId in ReaderIds)
                {
                    var properties = new Dictionary<string, string>();

                    var stack = CreateStack(readerId);

                    _logger.Info(string.Format(CultureInfo.InvariantCulture, "Executing reader {0}", stack.Reader.Id));
                    stack.Run(properties);
                }
            });
        }
    }
}
