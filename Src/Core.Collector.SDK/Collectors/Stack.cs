// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.DataModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Collector.SDK.Collectors
{
    public class Stack : AbstractStack
    {
        public override async Task HandleData(string senderId, List<IEntityCollection> data, Dictionary<string, string> context)
        {
            foreach (var transformer in Transformers)
            {
                await transformer.HandleData(senderId, data, context);
            }
        }

        public override async Task PublishData(string senderId, List<object> data, Dictionary<string, string> context)
        {
            foreach (var publisher in Publishers)
            {
                await publisher.PublishData(senderId, data, context);
            }
        }

        public override Task Run(Dictionary<string, string> properties)
        {
            var task = Task.Run(async () => await Reader.Run(properties), Reader.Token);
            return task;
        }
    }
}
