// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.Publishers;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using Collector.SDK.Collectors;
using Collector.SDK.Logging;
using System.Threading.Tasks;
using Collector.SDK.DataModel;
using System;
using Collector.SDK.Samples.DataModel;

namespace Collector.SDK.Samples.Publishers
{
    public class LogPublisher : AbstractPublisher
    {
        private ILogger _logger;
        private ICollector _collector;

        public LogPublisher(ILogger logger, ICollector collector) : base(collector)
        {
            _logger = logger;
            _collector = collector;
        }

        public override Task Publish(string senderId, List<object> data, Dictionary<string, string> context)
        {
            return Task.Run(() => {
                if (context.ContainsKey("Result") && context["Result"].Equals("Done"))
                {
                    // Handle the context, in this case we assume we are done.
                    var stateEventDone = new StateEvent()
                    {
                        SenderId = Id,
                        State = CollectorConstants.STATE_PUBLISHER_DONE,
                        ExtraInfo = "Publisher done."
                    };
                    _collector.SignalEvent(stateEventDone);
                }

                if (!EndPointConfig.Properties.ContainsKey(CollectorConstants.KEY_FOLDER))
                {
                    _logger.Error("Property 'Path' is missing from the end point config properties");
                    var stateEvent = new StateEvent()
                    {
                        SenderId = Id,
                        State = CollectorConstants.STATE_PUBLISHER_ERROR,
                        ExtraInfo = "Path property is missing."
                    };
                    _collector.SignalEvent(stateEvent);
                    return;
                }
                var path = EndPointConfig.Properties[CollectorConstants.KEY_FOLDER];
                if (!Directory.Exists(path))
                {
                    CreateDirectory(path);
                }
                var fullPath = string.Format("{0}\\publisher-log-{1}{2}{3}{4}{5}.txt",
                                    path, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Year, DateTime.Now.Hour, DateTime.Now.Minute);
                foreach (var dataPoint in data)
                {
                    var logEntry = dataPoint as LogEntry;
                    var payload = JsonConvert.SerializeObject(logEntry);
                    var line = string.Format("{0}\r\n", payload);
                    using (StreamWriter file = new StreamWriter(fullPath, File.Exists(fullPath)))
                    {
                        file.WriteLine(payload);
                    }
                }
            });
        }

        private void CreateDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                return;
            }
            Directory.CreateDirectory(path);
            var index = path.LastIndexOf('/');
            if (index <= 0)
                return;
            path = path.Substring(0, path.Length - index);
            CreateDirectory(path);
        }
    }
}
