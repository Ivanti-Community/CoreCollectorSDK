// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.Configuration;
using Collector.SDK.Readers;
using Collector.SDK.DataModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Threading;
using Collector.SDK.Collectors;

namespace Collector.SDK.Readers
{
    /// <summary>
    /// Provides convience methods that configures the reader and signals the handler with any data.
    /// </summary>
    public abstract class AbstractReader : IReader
    {
        private ICollector _collector;
        private string _id;
        private bool _done = false;
        private EndPointConfiguration _config;
        private IDataHandler _handler;
        private List<IEntityCollection> _data;
        private CancellationTokenSource _token;

        public string Id => _id;
        public bool Done => _done;
        public CancellationToken Token => _token.Token;
        public EndPointConfiguration EndPointConfig => _config;
        public IDataHandler Handler => _handler;
        public List<IEntityCollection> Data
        {
            get { return _data; }
            set { _data = value;  }
        }
        /// <summary>
        /// Inject the collector.
        /// </summary>
        /// <param name="collector"></param>
        public AbstractReader(ICollector collector)
        {
            _collector = collector;
            _token = new CancellationTokenSource();
        }
        /// <summary>
        /// Configure the connecter. 
        /// </summary>
        /// <param name="id">The id of this connector.</param>
        /// <param name="config">The configuration to use to connect to the database</param>
        /// <param name="handler">The callback to invoke when the connecter state changes</param>
        public virtual void Configure(string id, EndPointConfiguration config, IDataHandler handler)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");
            _id = id;

            _config = config ?? throw new ArgumentNullException("config");
            _handler = handler ?? throw new ArgumentNullException("handler");

            _data = new List<IEntityCollection>();
        }
        /// <summary>
        /// Signals the configured handler with the queried data.
        /// </summary>
        public async Task SignalHandler(Dictionary<string, string> context)
        {
            if (_handler == null)
                throw new ArgumentNullException("DataHandler");

            await _handler.HandleData(_id, _data, context);
        }
        /// <summary>
        /// Run the reader and signal completion when done.
        /// </summary>
        /// <param name="properties">Any runtime properties required by the reader.</param>
        /// <returns></returns>
        public async Task Run(Dictionary<string, string> properties)
        {
            await Read(properties);
            await SignalDone(properties);
        }
        /// <summary>
        /// Signal done and then set the cancellation token.
        /// </summary>
        public void Kill()
        {
            SignalDone(new Dictionary<string, string>()).Wait();
            _token.Cancel();
        }
        /// <summary>
        /// Signal that we are done.
        /// </summary>
        /// <param name="properties">Any runtime properties required by the reader.</param>
        /// <returns></returns>
        private async Task SignalDone(Dictionary<string, string> properties)
        {
            var context = new Dictionary<string, string>();
            context.Add(CollectorConstants.KEY_STATE, CollectorConstants.STATE_READER_DONE);
            await _handler.HandleData(_id, new List<IEntityCollection>(), context);
            _done = true;
            await _collector.SignalEvent(new StateEvent()
            {
                SenderId = Id,
                State = CollectorConstants.STATE_READER_DONE,
                ExtraInfo = properties
            });
        }

        public abstract void Dispose();
        public abstract Task Read(Dictionary<string, string> properties);
    }
}
