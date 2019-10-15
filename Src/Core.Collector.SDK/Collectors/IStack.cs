// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.Publishers;
using Collector.SDK.Readers;
using Collector.SDK.Transformers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Collector.SDK.Collectors
{
    public interface IStack : IDataHandler, ITransformedDataHandler
    {
        IReader Reader { get; }
        bool Done { get; set; }
        List<ITransformer> Transformers { get; }
        List<IPublisher> Publishers { get; }

        void Configure(IReader reader, List<ITransformer> transformers, List<IPublisher> publisher);

        Task Run(Dictionary<string, string> properties);

        void Kill();
    }
}
