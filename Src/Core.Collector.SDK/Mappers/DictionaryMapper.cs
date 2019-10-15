// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System;
using System.Collections.Generic;
using Collector.SDK.Collectors;
using Collector.SDK.Configuration;
using Collector.SDK.Converters;
using Collector.SDK.DataModel;

namespace Collector.SDK.Mappers
{
    public class DictionaryMapper : AbstractMapper
    {
        public override List<object> Map(List<IEntityCollection> data)
        {
            var convertedDataRows = new List<object>();
            foreach (var dataRow in data)
            {
                var mappedRow = new EntityCollection();
                foreach (var dataPoint in dataRow.Entities)
                {
                    if (!(dataPoint.Value is Dictionary<string, object>))
                    {
                        throw new ArgumentException("This mapper only converts dictionaries");
                    }
                    var dic = dataPoint.Value as Dictionary<string, object>;
                    foreach (var key in dic.Keys)
                    {
                        var convertedDataPoints = ConvertDataPoint(new KeyValuePair<string, object>(key, dic[key]), dataRow);
                        if (convertedDataPoints != null)
                        {
                            foreach (var convertedDataPoint in convertedDataPoints)
                            {
                                mappedRow.Entities.Add(convertedDataPoint.Key, convertedDataPoint.Value);
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(DataType))
                {
                    var entity = CollectorFactory.CreateEntity(DataType, mappedRow.Entities);
                    convertedDataRows.Add(entity);
                }
                else
                {
                    convertedDataRows.Add(mappedRow);
                }
            }
            return convertedDataRows;
        }
    }
}