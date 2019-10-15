// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************

using Collector.SDK.DataModel;

namespace Collector.SDK.Tests.Mocks
{
    public class MockUser : IEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string LastLogin { get; set; }
    }
}
