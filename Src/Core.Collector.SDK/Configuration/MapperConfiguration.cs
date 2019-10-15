// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System.Collections.Generic;

namespace Collector.SDK.Configuration
{
    public class MapperConfiguration
    {
        public MapperConfiguration()
        {
            Converters = new List<ConverterConfiguration>();
            PipedConverters = new List<ConverterConfiguration>();
        }
        public string Id { get; set; }
        public string DataType { get; set; }
        public string TransformerId { get; set; }
        public string Type { get; set; }
        public List<ConverterConfiguration> Converters { get; set; }
        public List<ConverterConfiguration> PipedConverters { get; set; }
        public List<SourceTargetMapping> SourceTargetMappings { get; set; }
    }
}
