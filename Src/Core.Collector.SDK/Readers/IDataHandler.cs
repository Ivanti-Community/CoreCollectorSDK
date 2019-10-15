// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.DataModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Collector.SDK.Readers
{
    /// <summary>
    /// Provides an interface for a handling data from (typically) a Reader.
    /// </summary>
    public interface IDataHandler
    {
        /// <summary>
        /// Handle a clock of data from a reader.
        /// </summary>
        /// <param name="senderId">The id of the reader.</param>
        /// <param name="data">The data to prcoess.</param>
        /// <param name="context">Context information that upper layers may need to know.</param>
        Task HandleData(string senderId, List<IEntityCollection> data, Dictionary<string, string> context);
    }
}
