// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.Logging;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Collector.SDK.Collectors
{
    public class SerialCollector : AbstractCollector
    {
        private readonly ILogger _logger;

        public SerialCollector(ILogger logger) : base(logger)
        {
            _logger = logger;
        }
        public override Task HandleEvent(StateEvent state)
        {
            return Task.Run(() => 
            {
                _logger.Info("ID = {0} - State = {1}", state.SenderId, state.State);
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
