// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System.Collections.Generic;

namespace Collector.SDK.Configuration
{
    public class CollectorConfiguration
    {
        public CollectorConfiguration()
        {
            ThirdPartyTypes = new List<ThirdPartyAutofacConfiguration>();
            ThirdPartyModules = new List<ThirdPartyAutofacConfiguration>();
            EndPoints = new List<EndPointConfiguration>();
            Readers = new List<ReaderConfiguration>();
            Transformers = new List<TransformerConfiguration>();
            Publishers = new List<PublisherConfiguration>();
            Properties = new Dictionary<string, string>();
        }
        public double Version { get; set; }
        public string Id { get; set; }
        public string Type { get; set; }
        public string StackType { get; set; }
        public List<ThirdPartyAutofacConfiguration> ThirdPartyTypes { get; set; }
        public List<ThirdPartyAutofacConfiguration> ThirdPartyModules { get; set; }
        public List<EndPointConfiguration> EndPoints { get; set; }
        public List<ReaderConfiguration> Readers { get; set; }
        public List<TransformerConfiguration> Transformers { get; set; }
        public List<PublisherConfiguration> Publishers { get; set; }
        public Dictionary<string, string> Properties { get; set; } 
    }
}
