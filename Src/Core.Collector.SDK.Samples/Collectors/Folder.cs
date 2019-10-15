// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System.IO;

namespace Collector.SDK.Samples.Collectors
{
    public class Folder : IDirectory
    {
        public string[] GetFiles(string folder, string filter)
        {
            return Directory.GetFiles(folder, filter);
        }
    }
}
