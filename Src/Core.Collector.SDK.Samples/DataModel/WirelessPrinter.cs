// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.DataModel;
using System;

namespace Collector.SDK.Samples.Transformers.DataModel
{
    public class WirelessPrinter : IEntity
    {
        public int Id { get; set; }
        public string PrinterName { get; set; }
        public string PrinterPlatform { get; set; }
        public DateTime CreateDateTime { get; set; }
        public NetworkCard NetworkCard { get; set; }
    }
}
