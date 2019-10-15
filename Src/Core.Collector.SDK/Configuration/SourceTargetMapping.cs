// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************

using System.Collections.Generic;

namespace Collector.SDK.Configuration
{
    public class SourceTargetMapping
    {
        public string PrimaryKey { get; set; }
        public List<SourceTargetConverter> TargetConverters { get; set; }
        public Dictionary<string, string> Properties { get; set; }

        public SourceTargetMapping()
        {
            Properties = new Dictionary<string, string>();
        }
    }
}
