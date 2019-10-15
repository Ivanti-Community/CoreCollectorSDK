// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System.Collections.Generic;
using Collector.SDK.Configuration;
using Collector.SDK.DataModel;

namespace Collector.SDK.Converters
{
    /// <summary>
    /// Provides convience methods to configure the converter.
    /// </summary>
    public abstract class AbstractConverter : IConverter
    {
        private string _id;
        private bool _combineInputOutput;
        private bool _nestOutput;
        private Dictionary<string, List<string>> _leftSideMap;
        private Dictionary<string, string> _properties;
        private SourceTargetMapping _mapping;
        private List<IConverter> _pipedConverters;

        public string Id => _id;
        public bool CombineInputOutput => _combineInputOutput;
        public bool NestOutput => _nestOutput;
        public Dictionary<string, List<string>> LeftSideMap => _leftSideMap;
        public Dictionary<string, string> Properties => _properties;
        public SourceTargetMapping Mapping => _mapping;
        public List<IConverter> PipedConverters => _pipedConverters;

        /// <summary>
        /// Configure the converter.
        /// </summary>
        /// <param name="config">The converter configuration.</param>
        public void Configure(ConverterConfiguration config)
        {
            _id = config.Id;
            _combineInputOutput = config.CombineInputOutput;
            _nestOutput = config.NestOutput;
            _leftSideMap = config.LeftSideMap;
            _properties = config.Properties;
            _mapping = config.Mapping;
            _pipedConverters = config.PipedConverters;
        }
        /// <summary>
        /// Convert the left side (name) of a data point.
        /// </summary>
        /// <param name="input">The name of the data point.</param>
        /// <returns>The output name of the converted value</returns>
        public List<string> ConvertLeftSide(string input)
        {
            if (_leftSideMap != null && _leftSideMap.ContainsKey(input))
                return _leftSideMap[input];

            return new List<string>() { input };
        }
        /// <summary>
        /// Convert a set of data points
        /// </summary>
        /// <param name="points">The points to convert</param>
        /// <param name="data">The associated data</param>
        /// <returns>The converted data</returns>
        public Dictionary<string, object> Convert(Dictionary<string, object> points, IEntityCollection data)
        {
            var result = new Dictionary<string, object>();
            foreach (var point in points)
            {
                if (LeftSideMap != null && LeftSideMap.ContainsKey(point.Key))
                {
                    var convertedDataPoints = Convert(point, data);
                    foreach (var key in convertedDataPoints.Keys)
                    {
                        result.Add(key, convertedDataPoints[key]);
                    }
                }
                else if (CombineInputOutput)
                {
                    result.Add(point.Key, point.Value);
                }
            }
            return result;
        }
        public abstract Dictionary<string, object> Convert(KeyValuePair<string, object> point, IEntityCollection data);
    }
}
