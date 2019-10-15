// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Collector.SDK.Transformers
{
    /// <summary>
    /// Handle data that has been transformed.
    /// </summary>
    public interface ITransformedDataHandler
    {
        /// <summary>
        /// Publish the data that has been transformed.
        /// </summary>
        /// <param name="senderId">The id of the sender.</param>
        /// <param name="data">The data to publish</param>
        /// <param name="context">Context information that upper layers may need to know.</param>
        Task PublishData(string senderId, List<object> data, Dictionary<string, string> context);
    }
}
