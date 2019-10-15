// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System.Collections.Generic;
using Collector.SDK.Collectors;
using Collector.SDK.DataModel;

namespace Collector.SDK.Mappers
{
    public class VersionOneMapper : AbstractMapper
    {
        /// <summary>
        /// The Default Mapper for configuration version 1.x
        /// </summary>
        /// <param name="data">The data to map</param>
        /// <returns>A list of converted data</returns>
        public override List<object> Map(List<IEntityCollection> data)
        {
            var convertedDataRows = new List<object>();
            foreach (var dataRow in data)
            {
                var mappedRow = new EntityCollection();
                foreach (var dataPoint in dataRow.Entities)
                {
                    if (Converters.ContainsKey(dataPoint.Key))
                    {
                        var converter = Converters[dataPoint.Key];
                        var convertedDataPoints = converter.Convert(dataPoint, dataRow);

                        if (convertedDataPoints != null)
                        {
                            foreach (var convertedDataPoint in convertedDataPoints)
                            {
                                mappedRow.Entities.Add(convertedDataPoint.Key, convertedDataPoint.Value);
                            }
                        }
                    }
                    else
                    {
                        mappedRow.Entities.Add(dataPoint.Key, dataPoint.Value);
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
