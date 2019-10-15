// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.Samples.Readers;

namespace Collector.SDK.Tests.Mocks
{
    public class MockFileReader : IFileReader
    {
        private int _lineCount = 0;

        public int LineCount => _lineCount;

        public bool Open(string fileName)
        {
            return true;
        }

        public string ReadLine()
        {
            if (_lineCount == 100)
            {
                ++_lineCount;
                return "2017-06-01 10:51:07,889 [1] INFO  XtractionServices.Configuration - Initializing...";
            }
            return null;
        }
    }
}
