// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System.Collections.Generic;

namespace Collector.SDK.DataModel
{
    /// <summary>
    /// Provides a container for rows of data points.
    /// </summary>
    public class EntityCollection : IEntityCollection
    {
        public Dictionary<string, object> _points;

        /// <summary>
        /// Create the data points dictionary.
        /// </summary>
        public EntityCollection()
        {
            _points = new Dictionary<string, object>();
        }
        /// <summary>
        /// A dictionary of data points as key/value pairs.
        /// </summary>
        public Dictionary<string, object> Entities
        {
            get { return _points; }
            set { _points = value; }
        }
    }
}
