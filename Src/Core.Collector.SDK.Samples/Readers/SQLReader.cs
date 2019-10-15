// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************
using System;
using System.Data;
using System.Data.SqlClient;

namespace Collector.SDK.Samples.Readers
{
    public class SQLReader : ISQLReader
    {
        private SqlConnection _connection;
        private SqlDataReader _dataReader;
        private SqlCommand _command;

        public bool Connect(string connectorString)
        {
            _connection = new SqlConnection();
            _connection.Open();

            return true;
        }

        public bool ExecuteStatement(string sqlStatement)
        {
            _command = new SqlCommand(sqlStatement, _connection);
            _dataReader = _command.ExecuteReader();
            if (_dataReader.Read())
            {
                if (_dataReader.HasRows)
                {
                    return true;
                }
            }
            return false;
        }

        public IDataRecord ReadNext()
        {
            IDataRecord record = _dataReader.GetEnumerator().Current as IDataRecord;
            _dataReader.GetEnumerator().MoveNext();
            return record;
        }
    }
}
