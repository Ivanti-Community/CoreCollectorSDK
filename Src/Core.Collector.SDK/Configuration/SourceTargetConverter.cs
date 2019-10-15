// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************

using System.Collections.Generic;

namespace Collector.SDK.Configuration
{
    public class SourceTargetConverter
    {
        public SourceTargetConverter()
        {
            Pipe = true;
            CombineInputOutput = true;
            NestOutput = false;
            Properties = new Dictionary<string, string>();
            InputFilter = new Dictionary<string, string>();
        }
        public string Id { get; set; }
        public bool CombineInputOutput { get; set; }
        public Dictionary<string, string> InputFilter { get; set; }
        public bool NestOutput { get; set; }
        public bool Pipe { get; set; }
        public bool InLeftSideMap { get; set; }
        public Dictionary<string, List<string>> LeftSideMap { get; set; }
        public Dictionary<string, string> Properties { get; set; }
    }
}
