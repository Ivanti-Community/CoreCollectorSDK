// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.Configuration;
using Collector.SDK.Readers;
using System;

namespace Collector.SDK.Transformers
{
    public interface ITransformer : IDataHandler, IDisposable
    {
        string Id { get; }
        bool Done { get; }
        TransformerConfiguration Config { get; }
        ITransformedDataHandler Handler { get; }
        void Configure(TransformerConfiguration config, ITransformedDataHandler handler);
    }
}
