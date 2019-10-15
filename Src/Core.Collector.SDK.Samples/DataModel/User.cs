// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.DataModel;
using System;

namespace Collector.SDK.Samples.DataModels
{
    public class User : IEntity
    {
        public string CommonName { get; set; }
        public string DistinguishedName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public DateTime Changed { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime ChangedUtc { get; set; }
    }
}
