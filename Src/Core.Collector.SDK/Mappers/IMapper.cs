// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.Configuration;
using Collector.SDK.Converters;
using Collector.SDK.DataModel;
using System;
using System.Collections.Generic;

namespace Collector.SDK.Mappers
{
    /// <summary>
    /// Transform data based on a set of converters.
    /// </summary>
    public interface IMapper
    {
        /// <summary>
        /// Id of the mapper.
        /// </summary>
        string Id { get; }
        /// <summary>
        /// The class as "[full class name],[assembly name]"
        /// </summary>
        string DataType { get; }
        /// <summary>
        /// The id of the transformer that uses this mapper.
        /// </summary>
        string TransformerId { get; }
        /// <summary>
        /// Configures the mapper.
        /// </summary>
        /// <param name="Id">The id of the mapper.</param>
        /// <param name="dataType">The class type.</param>
        /// <param name="transformerId">The transformer id</param>
        /// <param name="converters">The converters to convert the data points.</param>
        [Obsolete("This Configure is deprecated, please use Configure(MapperConfiguration config, Dictionary<string, IConverter> converters) instead.")]
        void Configure(string Id, string dataType, string transformerId, Dictionary<string, IConverter> converters);
        /// <summary>
        /// Configures the mapper.
        /// </summary>
        /// <param name="config">The mapper configuration to use.</param>
        /// <param name="converters">A dictionary of converters where the key is the id of the converter</param>
        void Configure(MapperConfiguration config, Dictionary<string, IConverter> converters);
        /// <summary>
        /// Map the data.
        /// </summary>
        /// <param name="data">The data to convert</param>
        /// <returns>The converted data.</returns>
        List<object> Map(List<IEntityCollection> data);
    }
}
