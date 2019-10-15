// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System.IO;

namespace Collector.SDK.Samples.Readers
{
    public class FileReader : IFileReader
    {
        private StreamReader _reader;
        public bool Open(string fileName)
        {
            _reader = File.OpenText(fileName);
            return true;
        }

        public string ReadLine()
        {
            return _reader.ReadLine();
        }
    }
}
