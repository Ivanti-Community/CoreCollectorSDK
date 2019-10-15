// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************

using Collector.SDK.DataModel;

namespace Collector.SDK.Samples.DataModels
{
    public class Location : IEntity
    {
        public string City { get; set; }
        public string State { get; set; }
    }
}
