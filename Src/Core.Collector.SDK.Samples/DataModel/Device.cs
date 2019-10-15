// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************

using Collector.SDK.DataModel;
using System;

namespace Collector.SDK.Samples.DataModels
{
    public class Device : IEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime Changed { get; set; }
        public DateTime ChangedUtc { get; set; }
        public DateTime Created { get; set; }
        public DateTime CreatedUtc { get; set; }
    }
}
