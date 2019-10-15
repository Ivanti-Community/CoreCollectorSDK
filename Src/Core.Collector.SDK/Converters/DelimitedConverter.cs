// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Collector.SDK.DataModel;

namespace Collector.SDK.Converters
{
    public class DelimitedConverter : AbstractConverter
    {
        public override Dictionary<string, object> Convert(KeyValuePair<string, object> point, IEntityCollection data)
        {
            var result = new Dictionary<string, object>();
            List<char> delimiters = new List<char>();
            if (Properties.ContainsKey("ArrayDelimeters"))
            {
                var dels = Properties["ArrayDelimeters"];
                for (int i = 0; i < dels.Length; i++)
                {
                    delimiters.Add(dels[i]);
                }
            }
            var array = ((string )point.Value).Split(delimiters.ToArray());
            for (int i = 0; i < array.Count(); i++ )
            {
                if (LeftSideMap.ContainsKey("" + i))
                {
                    var value = array[i];
                    var names = LeftSideMap["" + i];
                    foreach (var name in names)
                    {
                        result.Add(name, value);
                    }
                }
            }
            return result;
        }
    }
}
