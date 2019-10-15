// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System.Collections.Generic;

namespace Collector.SDK.Configuration
{
    public class EndPointConfiguration
    {
        public EndPointConfiguration()
        {
            Properties = new Dictionary<string, string>();
        }
        public string Id { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public Dictionary<string, string> Properties { get; set; }
    }
}
