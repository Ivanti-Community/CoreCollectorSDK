// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************

namespace Collector.SDK.Samples.Readers
{
    public interface IFileReader
    {
        bool Open(string fileName);
        string ReadLine();
    }
}
