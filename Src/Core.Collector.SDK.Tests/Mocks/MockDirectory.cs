// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.Samples.Collectors;
using System.Collections.Generic;
using System.Globalization;

namespace Collector.SDK.Tests.Mocks
{
    public class MockDirectory : IDirectory
    {
        private List<string> _files = new List<string>();
        public MockDirectory()
        {
            for (int i = 0; i < 100; i++)
            {
                _files.Add(string.Format(CultureInfo.InvariantCulture, "test_{0}.log", i));
            }
        }
        public string[] GetFiles(string folder, string filter)
        {
            return _files.ToArray();
        }
    }
}
