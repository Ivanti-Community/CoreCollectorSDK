// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.Configuration;
using Collector.SDK.DataModel;
using System.Collections.Generic;

namespace Collector.SDK.Converters
{
    /// <summary>
    /// Converts a data point.  Can do both left and right side conversion.
    /// </summary>
    public interface IConverter
    {
        string Id { get; }
        bool NestOutput { get; }
        Dictionary<string, List<string>> LeftSideMap { get; }
        Dictionary<string, string> Properties { get; }

        /// <summary>
        /// Configure the converter.
        /// </summary>
        /// <param name="config">The converters configuration.</param>
        void Configure(ConverterConfiguration config);
        /// <summary>
        /// Convert the data point.
        /// </summary>
        /// <param name="point">The data to convert.</param>
        /// <param name="data">The data this data point is in.</param>
        /// <returns>The converted data point.</returns>
        Dictionary<string, object> Convert(KeyValuePair<string, object> point, IEntityCollection data);
        /// <summary>
        /// Convert the data.  The data is previously converted data.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Dictionary<string, object> Convert(Dictionary<string, object> points, IEntityCollection data);
    }
}
