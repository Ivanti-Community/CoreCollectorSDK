// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************

namespace Collector.SDK.Collectors
{
    public class StateEvent
    {
        public string SenderId { get; set; }
        public string State { get; set; }
        public object ExtraInfo { get; set; }
    }
}
