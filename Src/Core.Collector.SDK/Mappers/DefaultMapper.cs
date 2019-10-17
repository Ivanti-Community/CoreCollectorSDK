// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System;
using System.Collections.Generic;
using Collector.SDK.Collectors;
using Collector.SDK.DataModel;
using Collector.SDK.Logging;

namespace Collector.SDK.Mappers
{
    public class DefaultMapper : AbstractMapper
    {
        private readonly ILogger _logger;
        private readonly ICollector _collector;

        /// <summary>
        /// Inject the logger.
        /// </summary>
        /// <param name="logger"></param>
        public DefaultMapper(ILogger logger, ICollector collector)
        {
            _logger = logger;
            _collector = collector;
        }
        /// <summary>
        /// Version 2.x mapping
        /// </summary>
        /// <param name="data">The data to convert</param>
        /// <returns>The converted data</returns>
        public override List<object> Map(List<IEntityCollection> data)
        {
            var convertedDataRows = new List<object>();
            try
            {
                foreach (var dataRow in data)
                {
                    var mappedRow = new EntityCollection();
                    foreach (var dataPoint in dataRow.Entities)
                    {
                        var convertedDataPoints = ConvertDataPoint(dataPoint, dataRow);
                        if (convertedDataPoints != null)
                        {
                            foreach (var convertedDataPoint in convertedDataPoints)
                            {
                                mappedRow.Entities.Add(convertedDataPoint.Key, convertedDataPoint.Value);
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
            } 
            catch (Exception e)
            {
                _logger.Error(e.Message);
            }
            return convertedDataRows;
        }
    }
}
