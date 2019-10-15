// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************

namespace Collector.SDK.Configuration
{
    public class ThirdPartyAutofacConfiguration
    {
        public ThirdPartyAutofacConfiguration()
        {
            RegisterAll = false;
            Owned = false;
            Singleton = false;
        }
        public string AssemblyName { get; set; }
        public bool RegisterAll { get; set; }
        public string Type { get; set; }
        public string Key { get; set; }
        public bool Owned { get; set; }
        public bool Singleton { get; set; }
    }
}
