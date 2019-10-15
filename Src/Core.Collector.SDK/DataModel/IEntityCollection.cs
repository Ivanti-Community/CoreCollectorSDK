// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System.Collections.Generic;

namespace Collector.SDK.DataModel
{
    /// <summary>
    /// Provides a container for rows of data points.
    /// </summary>
    public interface IEntityCollection
    {
        /// <summary>
        /// A dictionary of data points as key/value pairs.
        /// </summary>
        Dictionary<string, object> Entities
        {
            get;
            set;
        }
    }
}
