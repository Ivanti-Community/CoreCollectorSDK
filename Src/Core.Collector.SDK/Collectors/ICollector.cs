// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Collector.SDK.Collectors
{
    public interface ICollector
    {
        /// <summary>
        /// The id of this collector.
        /// </summary>
        string Id { get; }
        /// <summary>
        /// The version of the SDK.
        /// </summary>
        double Version { get; }
        /// <summary>
        /// Collector specific properties.
        /// </summary>
        Dictionary<string, string> Properties { get; }
        /// <summary>
        /// Configure the collector.
        /// </summary>
        /// <param name="config">The collector configuration to use.</param>
        /// <param name="connectors">The connectors to use.</param>
        void Configure(CollectorConfiguration config);
        /// <summary>
        /// Execute all the configured connectors.
        /// </summary>
        Task Run();
        /// <summary>
        /// Signal the director of a state event
        /// </summary>
        /// <param name="state">The state change event.</param>
        Task SignalEvent(StateEvent state);
    }
}
