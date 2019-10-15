// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************

using Collector.SDK.DataModel;
using Collector.SDK.Logging;
using System;
using System.Collections.Generic;

namespace Collector.SDK.Converters
{
	public class SplitConverter : AbstractConverter
	{
		private readonly ILogger _logger;
		public SplitConverter(ILogger logger)
		{
			_logger = logger;
		}

		public override Dictionary<string, object> Convert(KeyValuePair<string, object> point, IEntityCollection data)
		{
			char delimiter = ','; // default
			if (Properties.ContainsKey("Delimiter"))
				delimiter = Properties["Delimiter"][0];
			var convertedKeys = ConvertLeftSide(point.Key);
			Dictionary<string, object> result = new Dictionary<string, object>();
			var items = point.Value.ToString().Split(delimiter);

			//fill what it can and log error if items count doesn't match keys count
			if (items.Length != convertedKeys.Count)
				_logger.Error($"Key/Value mismatch while converting - key: {point.Key} - value: {point.Value} - delimiter: {delimiter}");

			for(int i = 0; i < Math.Min(items.Length, convertedKeys.Count); ++i)
			{
				result.Add(convertedKeys[i], items[i]);
			}

			return result;
		}
	}
}
