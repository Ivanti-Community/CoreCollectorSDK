// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.Samples.Readers;
using System.Data;

namespace Collector.SDK.Tests.Mocks
{
    public class MockSQLReader : ISQLReader
    {
        public bool Connect(string connectorString)
        {
            return true;
        }

        public bool ExecuteStatement(string command)
        {
            return true;
        }

        public IDataRecord ReadNext()
        {
            return null;
        }
    }
}
