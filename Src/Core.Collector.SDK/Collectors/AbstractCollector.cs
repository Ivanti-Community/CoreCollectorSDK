// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Collector.SDK.Configuration;
using Collector.SDK.Logging;

namespace Collector.SDK.Collectors
{
    /// <summary>
    /// Orchestrates multiple transformation and publication of data.
    /// </summary>
    public abstract class AbstractCollector : ICollector
    {
        private readonly ILogger _logger;

        protected CollectorConfiguration _config;
        private List<string> _readerIds;
        private Dictionary<string, string> _properties;
        private Dictionary<string, List<StateEvent>> _events = new Dictionary<string, List<StateEvent>>();
        private Semaphore _sema = new Semaphore(1, 1);
        private Dictionary<string, IStack> _stacks = new Dictionary<string, IStack>();

        public string Id => _config.Id;
        public double Version => _config.Version;
        public List<String> ReaderIds => _readerIds;
        public Dictionary<string, string> Properties => _properties;
 
        /// <summary>
        /// Inject the logger.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        public AbstractCollector(ILogger logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// Create all the readers, transformers and publishers.
        /// </summary>
        /// <param name="collectorConfig">The configuration to use.</param>
        public void Configure(CollectorConfiguration collectorConfig)
        {
            _config = collectorConfig;
            _properties = collectorConfig.Properties;
            _readerIds = new List<string>();
            foreach (var readerConfig in collectorConfig.Readers)
            {
                _readerIds.Add(readerConfig.Id);
            }
        }
        private IStack FindStackByPublisher(string publisherId)
        {
            foreach (var key in _stacks.Keys)
            {
                var stack = _stacks[key];
                foreach (var publisher in stack.Publishers)
                {
                    if (publisher.Id == publisherId)
                    {
                        return stack;
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// Create the entire stack.  Invoke run to execute the stack.
        /// </summary>
        /// <param name="readerId">The id of the reader.</param>
        /// <returns>A new stack</returns>
        public IStack CreateStack(string readerId)
        {
            var stack = CollectorFactory.CreateStack(readerId, _config);
            _sema.WaitOne();
            _stacks.Add(stack.Reader.Id, stack);
            _sema.Release();
            return stack;
        }
        /// <summary>
        /// Return the number of stacks the collector is tracking.
        /// </summary>
        /// <returns>Number of stacks the collector is tracking.</returns>
        public int GetStackCount()
        {
            var count = -1;
            _sema.WaitOne();
            count = _stacks.Count;
            _sema.Release();
            return count;
        }
        /// <summary>
        /// Remove a stack that is being tracked.
        /// </summary>
        /// <param name="readerId">The id of the reader associated with stack</param>
        public void RemoveStack(string readerId)
        {
            _sema.WaitOne();
            _stacks.Remove(readerId);
            _sema.Release();
        }
        /// <summary>
        /// Remove a stack that is being tracked.
        /// </summary>
        /// <param name="publisherId">The id of the publisher associated with a stack</param>
        public void RemoveStackByPublisher(string publisherId)
        {
            _sema.WaitOne();
            var stack = FindStackByPublisher(publisherId);
            if (stack != null) {
                _stacks.Remove(stack.Reader.Id);
            }
            _sema.Release();
        }
        /// <summary>
        /// Determines if a stack is complete based on a reader and all publishers having sent a done event.
        /// </summary>
        /// <param name="readerId">The id of the reader associated with stack</param>
        /// <returns>True if the stack is done.</returns>
        public bool StackDone(string readerId)
        {
            bool done = false;
            _sema.WaitOne();
            try
            {
                if (_stacks.ContainsKey(readerId))
                {
                    done = _stacks[readerId].Done;
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
            _sema.Release();
            return done;
        }
        /// <summary>
        /// Determines if a stack is done by the publisher id
        /// </summary>
        /// <param name="publisherId">The publisher id</param>
        /// <returns>True if the stack the publisher is in is done.</returns>
        public bool StackDoneByPublisher(string publisherId)
        {
            bool done = false;
            _sema.WaitOne();
            try
            {
                var stack = FindStackByPublisher(publisherId);
                if (stack != null)
                {
                    done = stack.Done;
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
            _sema.Release();
            return done;
        }
        /// <summary>
        /// Signal a state event
        /// </summary>
        /// <param name="state">The state</param>
        /// <returns>A task</returns>
        public Task SignalEvent(StateEvent state)
        {
            // Check if we need to unload the handlers for this sender.
            _logger.Debug("Event state signaled by {0}, state is {1}", state.SenderId, state.State);
            _sema.WaitOne();
            if (!_events.ContainsKey(state.SenderId))
            {
                _events.Add(state.SenderId, new List<StateEvent>());
            }
            else
            {
                _events[state.SenderId].Add(state);
            }

            if (state.State == CollectorConstants.STATE_READER_DONE)
            {
                if (_stacks.ContainsKey(state.SenderId))
                {
                    var stack = _stacks[state.SenderId];
                    bool allDone = true;
                    foreach (var publisher in stack.Publishers)
                    {
                        if (!publisher.Done)
                        {
                            allDone = false;
                            break;
                        }
                    }
                    stack.Done = allDone;
                }
            }
            _sema.Release();

            // Let the implementation have a go...
            return HandleEvent(state);
        }
        /// <summary>
        /// Run the collector.
        /// </summary>
        /// <returns>A task</returns>
        public abstract Task Run();
        /// <summary>
        /// Handle a state event
        /// </summary>
        /// <param name="state">The state to handle</param>
        /// <returns>A task</returns>
        public abstract Task HandleEvent(StateEvent state);
        /// <summary>
        /// Get the total number of events
        /// </summary>
        /// <returns>the total number of state events</returns>
        public int GetTotalEventCount()
        {
            int count = 0;
            _sema.WaitOne();
            count = _events.Count;
            _sema.Release();
            return count;
        }
        /// <summary>
        /// Get the total number of reader state done events
        /// </summary>
        /// <returns>the total number of state events</returns>
        public int GetReaderDoneCount()
        {
            int count = 0;
            _sema.WaitOne();
            foreach(var id in _events.Keys)
            {
                foreach (var state in _events[id])
                {
                    if (state.State == CollectorConstants.STATE_READER_DONE)
                    {
                        ++count;
                    }
                }
            }
            _sema.Release();
            return count;
        }
        /// <summary>
        /// Get the total number of transformer state done events
        /// </summary>
        /// <returns>the total number of state events</returns>
        public int GetTransformerDoneCount()
        {
            int count = 0;
            _sema.WaitOne();
            foreach (var id in _events.Keys)
            {
                foreach (var state in _events[id])
                {
                    if (state.State == CollectorConstants.STATE_TRANSFORMER_DONE)
                    {
                        ++count;
                    }
                }
            }
            _sema.Release();
            return count;
        }
        /// <summary>
        /// Get the total number of transformer state done events
        /// </summary>
        /// <returns>the total number of state events</returns>
        public int GetPublisherDoneCount()
        {
            int count = 0;
            _sema.WaitOne();
            foreach (var id in _events.Keys)
            {
                foreach (var state in _events[id])
                {
                    if (state.State == CollectorConstants.STATE_PUBLISHER_DONE)
                    {
                        ++count;
                    }
                }
            }
            _sema.Release();
            return count;
        }
        /// <summary>
        /// Get events by the sender id.
        /// </summary>
        /// <param name="senderId">A list of state events from the specified sender</param>
        /// <returns></returns>
        public List<StateEvent> GetEventsBySenderId(string senderId)
        {
            var stateEvents = new List<StateEvent>();
            _sema.WaitOne();
            if (_events.ContainsKey(senderId))
            {
                foreach (var state in _events[senderId])
                {
                    stateEvents.Add(state);
                }
            }
            _sema.Release();
            return stateEvents;
        }
    }
}
