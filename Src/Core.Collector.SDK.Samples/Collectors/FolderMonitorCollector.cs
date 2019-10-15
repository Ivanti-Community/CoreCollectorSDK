// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.Collectors;
using Collector.SDK.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Collector.SDK.Samples.Collectors
{
    public class FolderMonitorCollector : AbstractCollector
    {
        private readonly ILogger _logger;
        private IDirectory _folder;
        private DateTime _lastRead = DateTime.MinValue;
        private List<string> _runningReaders = new List<string>();
        private int _readerCount;
        private Semaphore _doneEvent = new Semaphore(0, 1);
        public int ReaderCount => _readerCount;

        public FolderMonitorCollector(IDirectory folder, ILogger logger) : base(logger)
        {
            _folder = folder;
            _logger = logger;
        }

        public override Task Run()
        {
            return Task.Run(() => {
                var path = Properties["FolderPath"];
                var filter = Properties["FileFilter"];
                string[] files = _folder.GetFiles(path, filter);
                _readerCount = files.Length; // assumes only one reader per file.
                foreach (var file in files)
                {
                    ProcessFile(file);
                }
                _doneEvent.WaitOne();
                _logger.Info("Done processing files for folder {0}!", path);
            });
        }

        private void ProcessFile(string fileName)
        {
            foreach (var readerId in ReaderIds)
            {
                var properties = new Dictionary<string, string>();
                properties.Add("FileName", fileName);

                var stack = CreateStack(readerId);

                _logger.Info(string.Format(CultureInfo.InvariantCulture, "Executing reader {0} for log file {1}", stack.Reader.Id, fileName));
                _runningReaders.Add(stack.Reader.Id);
                stack.Run(properties);
            }
        }

        public override async Task HandleEvent(StateEvent state)
        {
            await Task.Run(() => {
                _logger.Info(string.Format(CultureInfo.InvariantCulture, "Component {0} is {1}", state.SenderId, state.State));
                if (state.State == CollectorConstants.STATE_PUBLISHER_DONE)
                {
                    _logger.Info("Publisher done processing file {0}", state.ExtraInfo);
                }
                if (state.State == CollectorConstants.STATE_READER_DONE)
                {
                    if (StackDone(state.SenderId))
                    {
                        RemoveStack(state.SenderId);
                        _logger.Info("Stack is Done!");
                        if (GetStackCount() == 0)
                        {
                            _logger.Info("All stacks are complete!");
                            _doneEvent.Release();
                        }
                    }
                    _logger.Info("Reader done processing file {0}", state.ExtraInfo);
                }
            });
        }
    }
}
