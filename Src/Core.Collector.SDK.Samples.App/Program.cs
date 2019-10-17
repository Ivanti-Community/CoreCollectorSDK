using System;
using System.IO;
using Collector.SDK.Collectors;
using Collector.SDK.Configuration;
using Newtonsoft.Json;
using NLog;

namespace Core.Collector.SDK.Samples.App
{
    class Program
    {
        static void Main(string[] args)
        {
            /*var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "file.txt" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            // Apply config           
            NLog.LogManager.Configuration = config;*/

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
