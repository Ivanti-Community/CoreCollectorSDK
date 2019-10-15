// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************

using Collector.SDK.Configuration;
using Collector.SDK.Transformers;

namespace Collector.SDK.Publishers
{
    /// <summary>
    /// Publish data to some repository.
    /// </summary>
    public interface IPublisher : ITransformedDataHandler
    {
        /// <summary>
        /// The id of the publisher.
        /// </summary>
        string Id { get; }
        /// <summary>
        /// The information about the end point to publish to.
        /// </summary>
        EndPointConfiguration EndPointConfig { get; }
        /// <summary>
        /// True if the publisher is done.
        /// </summary>
        bool Done { get; }
        /// <summary>
        /// Configures the publisher.
        /// </summary>
        /// <param name="id">The id of the publisher.</param>
        /// <param name="config">The end point configuration to use to establish a connection to a data repo.</param>
        void Configure(string id, EndPointConfiguration config);
    }
}
