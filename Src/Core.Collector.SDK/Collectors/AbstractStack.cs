using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Collector.SDK.DataModel;
using Collector.SDK.Publishers;
using Collector.SDK.Readers;
using Collector.SDK.Transformers;

namespace Collector.SDK.Collectors
{
    public abstract class AbstractStack : IStack
    {
        private IReader _reader;
        private bool _readerDone = false;
        private List<ITransformer> _transformers;
        private List<IPublisher> _publishers;

        public IReader Reader => _reader;
        public bool Done
        {
            get { return _readerDone; }
            set { _readerDone = value; }
        }
        public List<ITransformer> Transformers => _transformers;
        public List<IPublisher> Publishers => _publishers;
        /// <summary>
        /// Configure the reader.
        /// </summary>
        /// <param name="reader">The reader to clone.</param>
        /// <param name="transformers">The transformers to clone.</param>
        /// <param name="publishers">The publishers to clone.</param>
        public void Configure(IReader reader, List<ITransformer> transformers, List<IPublisher> publishers)
        {
            _reader = reader;
            _transformers = transformers;
            _publishers = publishers;
        }
        /// <summary>
        /// Set the cancellation token on the reader.
        /// </summary>
        public void Kill()
        {
            Reader.Kill();
        }

        public abstract Task HandleData(string senderId, List<IEntityCollection> data, Dictionary<string, string> context);
        public abstract Task PublishData(string senderId, List<object> data, Dictionary<string, string> context);
        public abstract Task Run(Dictionary<string, string> properties);
    }
}
