// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using Collector.SDK.Collectors;
using Collector.SDK.DataModel;
using Collector.SDK.Logging;
using Collector.SDK.Readers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;

namespace Collector.SDK.Samples.Readers
{
    /// <summary>
    /// Queries a SQL Server database
    /// </summary>
    public class SQLPrinterReader : AbstractReader
    {
        private readonly ILogger _logger;
        private ICollector _collector;
        private ISQLReader _sqlReader;
        /// <summary>
        /// Inject the logger.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="collector">The collector to signal state eevnts</param>
        public SQLPrinterReader(ISQLReader sqlReader, ILogger logger, ICollector collector) : base(collector)
        {
            _sqlReader = sqlReader;
            _logger = logger;
            _collector = collector;
        }
        /// <summary>
        /// Read a page/block of data.
        /// </summary>
        /// <param name="connection">The SQL Connection to the database.</param>
        /// <param name="sqlCommand">The SQL command to execute.</param>
        /// <param name="skip">The number of rows to skip.</param>
        /// <param name="top">The maximum number of rows to return.</param>
        /// <returns></returns>
        private List<IEntityCollection> ReadPage(string sqlCommand, int skip, int top)
        {
            List<IEntityCollection> result = new List<IEntityCollection>();

            var sqlCommandEx = sqlCommand;
            // If we are paging (which we should be), then concat the Offset and Fetch Next to the SQL query.
            sqlCommandEx = string.Format(CultureInfo.InvariantCulture, "{0} OFFSET {1} ROWS FETCH NEXT {2} ROWS ONLY;",
                sqlCommand, skip, top);

            // Create the command and execute
            IDataRecord record;
            while ((record = _sqlReader.ReadNext()) != null)
            {
                var row = new EntityCollection();
                for (int index = 0; index < record.FieldCount; index++)
                {
                    // Check if the data is null
                    // TODO this should be done with a mapper.
                    var value = record.GetValue(index);
                    var type = record.GetFieldType(index);
                    if (value == DBNull.Value)
                    {
                        if (type.Equals(typeof(int)))
                        {
                            value = 0;
                        }
                        else if (type.Equals(typeof(string)))
                        {
                            value = "";
                        }
                        else if (type.Equals(typeof(double)))
                        {
                            value = 0.0;
                        }
                        else if (type.Equals(typeof(DateTime)))
                        {
                            value = DateTime.Now;
                        }
                    }
                    row.Entities.Add(record.GetName(index), value);
                }
                result.Add(row);
            }

            return result;
        }
        /// <summary>
        /// Verify that all the required Data Source Configuration properties are present otherwise throw ArgumentNullException.
        /// </summary>
        private void VerifyConfiguration()
        {
            if (string.IsNullOrEmpty(EndPointConfig.User))
                throw new ArgumentNullException("User");

            if (string.IsNullOrEmpty(EndPointConfig.Password))
                throw new ArgumentNullException("Password");

            foreach (var key in EndPointConfig.Properties.Keys)
            {
                _logger.Debug(string.Format(CultureInfo.InvariantCulture, "{0} : {1}", key, EndPointConfig.Properties[key]));
            }
            if (string.IsNullOrEmpty(EndPointConfig.Properties["ServerName"]))
                throw new ArgumentNullException("ServerName");

            if (string.IsNullOrEmpty(EndPointConfig.Properties["Database"]))
                throw new ArgumentNullException("Database");

            if (string.IsNullOrEmpty(EndPointConfig.Properties["Top"]))
                throw new ArgumentNullException("Top");
        }
        /// <summary>
        /// Extract the data from the endpoint.
        /// </summary>
        public override async Task Read(Dictionary<string, string> properties)
        {
            var context = new Dictionary<string, string>();
            try
            {
                VerifyConfiguration();

                // Create the connection string.
                var serverName = EndPointConfig.Properties["ServerName"];
                var database = EndPointConfig.Properties["Database"];
                var sqlCommand = EndPointConfig.Properties["SqlCommand"];

                var top = int.Parse(EndPointConfig.Properties["Top"]);
                var skip = 0;

                _logger.Info(string.Format(CultureInfo.InvariantCulture, "Connecting to {0} : {1}",
                    serverName, database));

                var connectionString = string.Format(CultureInfo.InvariantCulture,
                    "Server={0};Database={1};Integrated Security=False;User Id={2};Password={3};MultipleActiveResultSets=True",
                    serverName,
                    database,
                    EndPointConfig.User,
                    EndPointConfig.Password);

                if (_sqlReader.Connect(connectionString))
                {
                    // Read page by page till there are no more rows to read.
                    while (true)
                    {
                        Data = ReadPage(sqlCommand, skip, top);
                        skip += Data.Count;

                        // Signal the handler with this page of data.
                        await SignalHandler(context);

                        if (Data.Count < top)
                            break;
                    }
                    _logger.Info(string.Format(CultureInfo.InvariantCulture, "Disconnecting from {0} : {1}",
                        EndPointConfig.Properties["ServerName"],
                        EndPointConfig.Properties["Database"]));
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
            }
        }
        public override void Dispose()
        {
            // nada
        }
    }
}
