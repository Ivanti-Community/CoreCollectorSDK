// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.Collectors;
using Collector.SDK.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Collector.SDK.Readers
{
    /// <summary>
    /// Reads data from a data source based on the end point configuration and passes that data onto a handler.
    /// </summary>
    public interface IReader : IStackLayer, IDisposable
    {
        string Id { get; }
        bool Done { get; }
        EndPointConfiguration EndPointConfig { get; }
        IDataHandler Handler { get; }

        /// <summary>
        /// Configure the reader.
        /// </summary>
        /// <param name="id">The id of the reader.</param>
        /// <param name="config">The end point configuration to use.</param>
        /// <param name="handler">The handler to signal with queried data.</param>
        void Configure(string id, EndPointConfiguration config, IDataHandler handler);
        /// <summary>
        /// Run the reader.
        /// </summary>
        /// <param name="properties">Run time properties for the reader.</param>
        /// <returns></returns>
        Task Run(Dictionary<string, string> properties);
    }
}
