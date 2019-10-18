// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.Collectors;
using Collector.SDK.Configuration;
using Collector.SDK.Converters;
using Collector.SDK.DataModel;
using System.Collections.Generic;

namespace Collector.SDK.Mappers
{
    public abstract class AbstractMapper : IMapper
    {
        private string _id;
        private string _dataType;
        private string _transformerId;
        private Dictionary<string, IConverter> _converters;
        private List<SourceTargetMapping> _mappings;

        public string Id => _id;
        public string DataType => _dataType;
        public string TransformerId => _transformerId;
        public Dictionary<string, IConverter> Converters => _converters;
        public List<SourceTargetMapping> Mappings => _mappings;

        public void Configure(string Id, string dataType, string transformerId, Dictionary<string, IConverter> converters)
        {
            _id = Id;
            _dataType = dataType;
            _transformerId = transformerId;
            _converters = converters;
        }

        public void Configure(MapperConfiguration config, Dictionary<string, IConverter> converters)
        {
            _id = config.Id;
            _dataType = config.DataType;
            _transformerId = config.TransformerId;
            _mappings = config.SourceTargetMappings;
            _converters = converters;
        }

        /// <summary>
        /// Find the source target mapping based on a primary key.
        /// </summary>
        /// <param name="key">The primary key.</param>
        /// <returns>The mapping if found, otherwise null</returns>
        public SourceTargetMapping FindMapping(string key)
        {
            foreach (var mapping in Mappings)
            {
                var primaryKey = mapping.PrimaryKey;

                if (primaryKey.Equals("*"))
                {
                    foreach (var converter in mapping.TargetConverters)
                    {
                        if (converter.LeftSideMap.ContainsKey(key))
                        {
                            return mapping;
                        }
                    }
                }
                else if (primaryKey.Contains("*"))
                {
                    primaryKey = primaryKey.Substring(0, primaryKey.IndexOf("*"));
                    if (primaryKey.Length == 0 || key.StartsWith(primaryKey))
                    {
                        return mapping;
                    }
                }
                else if (primaryKey.Equals(key))
                {
                    return mapping;
                }
            }
            return null;
        }

        /// <summary>
        /// Handle a nested conversion of data points.
        /// </summary>
        /// <param name="convertedDataPoints">The data to convert</param>
        /// <param name="dataRow">Any assocaited data.</param>
        /// <returns></returns>
        public Dictionary<string, object> NestedConversion(Dictionary<string, object> convertedDataPoints, IEntityCollection dataRow)
        {
            if (convertedDataPoints != null)
            {
                var newConvertedDataPoints = new Dictionary<string, object>();
                foreach (var point in convertedDataPoints)
                {
                    dataRow.Entities.Add(point.Key, point.Value);
                }
                foreach (var point in convertedDataPoints)
                {
                    var nestedDataPoints = ConvertDataPoint(point, dataRow);
                    if (nestedDataPoints != null)
                    {
                        foreach (var nestedDataPoint in nestedDataPoints)
                        {
                            newConvertedDataPoints.Add(nestedDataPoint.Key, nestedDataPoint.Value);
                        }
                    }
                }
                return newConvertedDataPoints;
            }
            return convertedDataPoints;
        }

        /// <summary>
        /// Pipe the data based on two rules.  
        ///     1. pipeData=true, piped in data is lost. 
        ///     2. pipeData=false, piped in data is combined with the output.
        /// </summary>
        /// <param name="combineInputOutput">Which pipe rule to use.</param>
        /// <param name="convertedDataPointsIn">The input</param>
        /// <param name="convertedDataPointsOut">The output</param>
        /// <returns>The output based on the piping rules.</returns>
        public Dictionary<string, object> CombineData(bool combineInputOutput,
            Dictionary<string, object> convertedDataPointsIn, Dictionary<string, object> convertedDataPointsOut)
        {
            var convertedDataPoints = new Dictionary<string, object>();
            if (convertedDataPointsOut != null && !combineInputOutput)
            {
                // Add only the output, input is thrown away.
                convertedDataPoints = convertedDataPointsOut;
            }
            else
            {
                // Add both input and output
                if (convertedDataPointsIn != null)
                {
                    foreach (var p in convertedDataPointsIn)
                    {
                        if (!convertedDataPoints.ContainsKey(p.Key))
                        {
                            convertedDataPoints.Add(p.Key, p.Value);
                        }
                    }
                }
                if (convertedDataPointsOut != null)
                {
                    foreach (var p in convertedDataPointsOut)
                    {
                        if (!convertedDataPoints.ContainsKey(p.Key))
                        {
                            convertedDataPoints.Add(p.Key, p.Value);
                        }
                    }
                }
            }
            return convertedDataPoints;
        }

	    /// <summary>
	    /// Merge two property sets.
	    /// </summary>
	    /// <param name="source1">This source will override any properties in source2</param>
	    /// <param name="source2">Additional properties</param>
	    /// <returns>source1 and source2 merged, if there are any conflicts source1 wins</returns>
	    public Dictionary<string, string> MergeProperties(Dictionary<string, string> source1, Dictionary<string, string> source2)
	    {
		    var result = new Dictionary<string, string>();
		    foreach (var key in source1.Keys)
		    {
			    result.Add(key, source1[key]);
		    }
		    foreach (var key in source2.Keys)
		    {
			    if (!result.ContainsKey(key))
			    {
				    result.Add(key, source2[key]);
			    }
		    }
		    return result;
	    }

        /// <summary>
        /// Find the converter based on its configuration mapping.
        /// </summary>
        /// <param name="dataPointKey">The data point to convert</param>
        /// <param name="targetConverter">The converter's config</param>
        /// <param name="converters">A list of configured converters</param>
        /// <returns>The converter if found, otherwise null.</returns>
        public IConverter MatchTargetConverter(string dataPointKey, SourceTargetConverter targetConverter, List<IConverter> converters)
        {
            IConverter converter = null;
            foreach (var c in converters)
            {
                if (c.Id.Equals(targetConverter.Id))
                {
                    if (targetConverter.InLeftSideMap)
                    {
                        // Check if the property is in the left side mapping
                        foreach (var key in c.LeftSideMap.Keys)
                        {
                            if (key.Equals(dataPointKey))
                            {
                                converter = c;
                                break;
                            }
                        }
                        if (converter != null)
                        {
                            break;
                        }
                    }
                    else
                    {
                        converter = c;
                        break;
                    }
                }
            }
            return converter;
        }

        /// <summary>
        /// Convert a single data point.
        /// </summary>
        /// <param name="dataPoint">The data point to convert</param>
        /// <param name="dataRow">The associated row data points</param>
        /// <returns>A dictionary of converted data.</returns>
        public Dictionary<string, object> ConvertDataPoint(KeyValuePair<string, object> dataPoint, IEntityCollection dataRowIn)
        {
            Dictionary<string, object> result = null;
            var mapping = FindMapping(dataPoint.Key);
            if (mapping != null)
            {
                // make our local copy of the data row, we may need to manipulate it for nested conversions.
                var dataRow = new EntityCollection();
                foreach (var e in dataRowIn.Entities)
                {
                    dataRow.Entities.Add(e.Key, e.Value);
                }
                var converters = new List<IConverter>();
                // first configure all of the trageted converters, 
                // this in case a piped converter wants to run the other converters internally
                foreach (var targetConverter in mapping.TargetConverters)
                {
                    if (Converters.ContainsKey(targetConverter.Id))
                    {
                        var mergedProperties = MergeProperties(Converters[targetConverter.Id].Properties, targetConverter.Properties);
                        mergedProperties = MergeProperties(mergedProperties, mapping.Properties);
                        var config = new ConverterConfiguration()
                        {
                            Id = targetConverter.Id,
                            CombineInputOutput = targetConverter.CombineInputOutput,
                            NestOutput = targetConverter.NestOutput,
                            LeftSideMap = targetConverter.LeftSideMap,
	                        Properties = mergedProperties,
							PipedConverters = converters,
                            Mapping = mapping
                        };
                        IConverter converter = CollectorFactory.CloneConverter(Converters[targetConverter.Id]);
                        if (converter != null)
                        {
                            converter.Configure(config);
                            converters.Add(converter);
                        }
                    }
                }
                Dictionary<string, object> convertedDataPoints = null;
                foreach (var targetConverter in mapping.TargetConverters)
                {
                    IConverter converter = MatchTargetConverter(dataPoint.Key, targetConverter, converters);
                    if (converter != null)
                    {
                        Dictionary<string, object> convertedDataPointsOut = null;
                        if (convertedDataPoints != null)
                        {
                            convertedDataPointsOut = converter.Convert(convertedDataPoints, dataRow);
                        }
                        else
                        {
                            convertedDataPointsOut = converter.Convert(dataPoint, dataRow);
                            // If we need to combine input and output, and this is our first converter.
                            // Otherwise the data point will be lost.
                            if (targetConverter.CombineInputOutput)
                            {
                                convertedDataPoints = new Dictionary<string, object>();
                                convertedDataPoints.Add(dataPoint.Key, dataPoint.Value);
                            }
                        }
                        convertedDataPoints = CombineData(targetConverter.CombineInputOutput, convertedDataPoints, convertedDataPointsOut);
                        if (targetConverter.NestOutput)
                        {
                            convertedDataPointsOut = NestedConversion(convertedDataPoints, dataRow);
                            convertedDataPoints = CombineData(targetConverter.CombineInputOutput, convertedDataPoints, convertedDataPointsOut);
                        }
                        if (!targetConverter.Pipe)
                        {
                            break;
                        }
                    }
                }
                result = convertedDataPoints;
            }
            return result;
        }

        public abstract List<object> Map(List<IEntityCollection> data);
    }
}
