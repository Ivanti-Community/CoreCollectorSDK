// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.Collectors;
using Collector.SDK.DataModel;
using Collector.SDK.Logging;
using Collector.SDK.Readers;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Collector.SDK.Samples.Readers
{
    public class LogFileReader : AbstractReader
    {
        private readonly ILogger _logger;
        private ICollector _collector;
        private IFileReader _fileReader;

        public LogFileReader(IFileReader fileReader, ILogger logger, ICollector collector) : base(collector)
        {
            _fileReader = fileReader;
            _logger = logger;
            _collector = collector;
        }

        public override void Dispose()
        {
            // nada
        }

        public override async Task Read(Dictionary<string, string> properties)
        {
            var fileName = properties[CollectorConstants.KEY_FILENAME];

            _logger.Info("Reading log file : " + fileName);
            // context gets bubbled up to the transformer and possibly the publisher
            var context = new Dictionary<string, string>();
            context.Add(CollectorConstants.KEY_FILENAME, fileName);

            if (!string.IsNullOrEmpty(fileName))
            {
                _logger.Info("Processing file {0}", fileName);
                if (_fileReader.Open(fileName))
                {
                    string s = "";
                    while ((s = _fileReader.ReadLine()) != null)
                    {
                        // Each row is a line of text from the log
                        var row = new EntityCollection();
                            
                        // Split the log entry by spaces
                        var entries = s.Split(' ');
                        if (entries.Length > 6)
                        {
                            // TODO: How to handle exceptions in the log?
                            if (entries[0].Contains("Exception"))
                                continue;
                            // From entry 6 on is the message, lets recreate it...
                            var message = "";
                            for (int i = 6; i < entries.Length; i++)
                            {
                                message = string.Format(CultureInfo.InvariantCulture, "{0} {1}", message, entries[i]);
                            }
                            // Now create our data entry...
                            row.Entities.Add("Date", entries[0]);
                            row.Entities.Add("Time", entries[1]);
                            row.Entities.Add("Type", entries[3]);
                            row.Entities.Add("Module", entries[5]);
                            row.Entities.Add("Message", message);
                            Data.Add(row);

                            // Tell the collector that we are done
                            await SignalHandler(context);
                        }
                    }
                }
            }
        }
    }
}
