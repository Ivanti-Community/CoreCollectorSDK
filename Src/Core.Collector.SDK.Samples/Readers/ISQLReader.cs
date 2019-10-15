// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System.Data;

namespace Collector.SDK.Samples.Readers
{
    public interface ISQLReader
    {
        bool Connect(string connectorString);
        bool ExecuteStatement(string command);
        IDataRecord ReadNext();
    }
}
