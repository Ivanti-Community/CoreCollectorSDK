// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************

using System.Collections.Generic;

namespace Collector.SDK.Configuration
{
    public class TransformerConfiguration
    {
        public TransformerConfiguration()
        {
            Mappers = new List<MapperConfiguration>();
        }
        public string Id { get; set; }
        public string Type { get; set; }
        public string ReaderId { get; set; }
        public List<MapperConfiguration> Mappers { get; set; }
    }
}
