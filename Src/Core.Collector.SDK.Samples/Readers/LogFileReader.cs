// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.Collectors;
using Collector.SDK.DataModel;
using Collector.SDK.Logging;
using Collector.SDK.Readers;
using System;
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

        public LogFileReader(ILogger logger, ICollector collector) : base(collector)
        {
            _fileReader = new FileReader();
            _logger = logger;
            _collector = collector;
        }

        public override void Dispose()
        {
            // nada
        }

        public override async Task Read(Dictionary<string, string> properties)
        {
            try
            {
                if (!EndPointConfig.Properties.ContainsKey(CollectorConstants.KEY_FOLDER))
                {
                    _logger.Error("Property 'FolderName' is missing from the end point config properties");
                    var stateEvent = new StateEvent()
                    {
                        SenderId = Id,
                        State = CollectorConstants.STATE_READER_ERROR,
                        ExtraInfo = "FolderName property is missing."
                    };
                    await _collector.SignalEvent(stateEvent);
                    return;
                }
                if (!EndPointConfig.Properties.ContainsKey(CollectorConstants.KEY_FILENAME))
                {
                    _logger.Error("Property 'FileName' is missing from the end point config properties");
                    var stateEvent = new StateEvent()
                    {
                        SenderId = Id,
                        State = CollectorConstants.STATE_READER_ERROR,
                        ExtraInfo = "FileName property is missing."
                    };
                    await _collector.SignalEvent(stateEvent);
                    return;
                }
                var path = EndPointConfig.Properties[CollectorConstants.KEY_FOLDER];
                var fileName = EndPointConfig.Properties[CollectorConstants.KEY_FILENAME];
                var fullPath = string.Format(@"{0}\{1}", path, fileName);

                _logger.Info("Reading log file : " + fullPath);
                // context gets bubbled up to the transformer and possibly the publisher
                var context = new Dictionary<string, string>();
                context.Add(fullPath, fileName);

                _logger.Info("Processing file {0}", fileName);
                if (_fileReader.Open(fullPath))
                {
                    string s = "";
                    while ((s = _fileReader.ReadLine()) != null)
                    {
                        // Each row is a line of text from the log
                        var row = new EntityCollection();

                        // Split the log entry by spaces
                        var entries = s.Split(' ', '|');
                        if (entries.Length > 6)
                        {
                            // TODO: How to handle exceptions in the log?
                            if (entries[0].Contains("Exception"))
                                continue;
                            // From entry 6 on is the message, lets recreate it...
                            var message = "";
                            for (int i = 4; i < entries.Length; i++)
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
            catch (Exception e)
            {
                _logger.Error(e.Message);
            }
        }
    }
}
