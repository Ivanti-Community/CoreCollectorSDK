// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Collector.SDK.Collectors;
using Collector.SDK.Configuration;
using Collector.SDK.DataModel;
using Collector.SDK.Mappers;

namespace Collector.SDK.Transformers
{
    /// <summary>
    /// Provides convience methods for configuration of a transformer.
    /// </summary>
    public abstract class AbstractTransformer : ITransformer
    {
        private ICollector _collector;
        private TransformerConfiguration _config;
        private List<IMapper> _mappers;
        private ITransformedDataHandler _handler;
        private List<object> _data;
        private bool _done = false;

        public string Id => _config.Id;
        public bool Done => _done;
        public TransformerConfiguration Config => _config;
        public ITransformedDataHandler Handler => _handler;
        public List<IMapper> Mappers {
            get { return _mappers; }
        }
        public List<object> Data
        {
            get { return _data; }
            set { _data = value; }
        }
        /// <summary>
        /// Inject the collector.
        /// </summary>
        /// <param name="collector"></param>
        public AbstractTransformer(ICollector collector)
        {
            _collector = collector;
        }
        /// <summary>
        /// Configure the transformer.
        /// </summary>
        /// <param name="config">The configuration to use.</param>
        /// <param name="handler">The handler to invoke once the data has been transformed.</param>
        public virtual void Configure(TransformerConfiguration config, ITransformedDataHandler handler)
        {
            _config = config;
            _mappers = new List<IMapper>();
            _data = new List<object>();

            foreach (var mapperConfig in _config.Mappers)
            {
                var mapper = CollectorFactory.CreateMapper(mapperConfig);
                if (mapper == null)
                    throw new NullReferenceException("Unable to create Mapper.  Invalid Mapper defined.");

                _mappers.Add(mapper);
            }
            _handler = handler;
        }
        public async Task SignalPublisher(Dictionary<string, string> context)
        {
            if (_handler == null)
                throw new ArgumentNullException("DataHandler");

            await _handler.PublishData(Id, Data, context);
        }
        public void Dispose()
        {
            // nada
        }
        public async Task HandleData(string senderId, List<IEntityCollection> data, Dictionary<string, string> context)
        {
            if (data != null && data.Count != 0)
            {
                await Transform(senderId, data, context);
            }

            // Only signal tha we are done when the reader is done.
            if (context.ContainsKey(CollectorConstants.KEY_STATE))
            {
                if (context[CollectorConstants.KEY_STATE] == CollectorConstants.STATE_READER_DONE)
                {
                    _done = true;
                    await _collector.SignalEvent(new StateEvent()
                    {
                        SenderId = Id,
                        State = CollectorConstants.STATE_TRANSFORMER_DONE,
                        ExtraInfo = context
                    });
                    await _handler.PublishData(Id, new List<object>(), context);
                }
            }
        }

        public abstract Task Transform(string senderId, List<IEntityCollection> data, Dictionary<string, string> context);
    }
}
