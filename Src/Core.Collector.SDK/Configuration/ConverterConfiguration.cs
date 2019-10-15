// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************

using Collector.SDK.Converters;
using System.Collections.Generic;

namespace Collector.SDK.Configuration
{
    public class ConverterConfiguration
    {
        public ConverterConfiguration()
        {
            NestOutput = false;
            CombineInputOutput = false;
            Properties = new Dictionary<string, string>();
            LeftSideMap = new Dictionary<string, List<string>>();
            PipedConverters = new List<IConverter>();
        }
        public string Id { get; set; }
        public bool CombineInputOutput { get; set; }
        public bool NestOutput { get; set; }
        public string DataType { get; set; }
        public Dictionary<string, List<string>> LeftSideMap { get; set; }
        public string Type { get; set; }
        public Dictionary<string, string> Properties { get; set; }
        public SourceTargetMapping Mapping { get; set; }
        public List<IConverter> PipedConverters { get; set; }
    }
}
