// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.Collectors;
using Collector.SDK.Converters;
using Collector.SDK.DataModel;
using Collector.SDK.Mappers;
using System.Collections.Generic;

namespace Collector.SDK.Tests.Mocks
{
    public class MockMapper : AbstractMapper
    {
        public override List<object> Map(List<IEntityCollection> data)
        {
            var result = new List<object>();
            foreach (var row in data)
            {
                var entityProperties = new Dictionary<string, object>();
                foreach (var point in row.Entities)
                {
                    if (Converters.ContainsKey(point.Key))
                    {
                        var newPoints = ((IConverter)Converters[point.Key]).Convert(point, row);
                        foreach (var newPoint in newPoints)
                        {
                            entityProperties.Add(newPoint.Key, newPoint.Value);
                        }
                    }
                }
                result.Add(CollectorFactory.CreateEntity(DataType, entityProperties));
            }
            return result;
        }
    }
}
