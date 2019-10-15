// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************

namespace Collector.SDK.Samples.Collectors
{
    public interface IDirectory
    {
        string[] GetFiles(string folder, string filter);
    }
}
