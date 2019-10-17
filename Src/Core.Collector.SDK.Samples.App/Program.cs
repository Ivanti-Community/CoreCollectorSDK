using System;
using System.IO;
using Collector.SDK.Collectors;
using Collector.SDK.Configuration;
using Newtonsoft.Json;

namespace Core.Collector.SDK.Samples.App
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Example Log File Collector.");
            using (StreamReader file = File.OpenText(@"collector-config-piped-converter.json"))
            {
                var serializer = new JsonSerializer();
                // convert from json to the collector configuration object
                var collectorConfig = (CollectorConfiguration)serializer.Deserialize(file, typeof(CollectorConfiguration));
                // instantiate the collector
                var collector = CollectorFactory.CreateCollector(collectorConfig);
                // Run it...
                collector.Run();
                Console.WriteLine("Hit any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
