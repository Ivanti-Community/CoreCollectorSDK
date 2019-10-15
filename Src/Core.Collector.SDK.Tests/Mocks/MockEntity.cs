// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************

using Collector.SDK.DataModel;

namespace Collector.SDK.Tests.Mocks
{
    public class MockEntity : IEntity
    {
        public string Id { get; set; }
        public string ABC { get; set; }
        public string XYZ { get; set; }
    }
}
