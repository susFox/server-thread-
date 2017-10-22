using System;
using System.Data;

namespace Server_Thread_
{
    internal class MySqlDataAdapter
    {
        private string dbQuery;
        private MySqlConnection dbConn;

        public MySqlDataAdapter(string dbQuery, MySqlConnection dbConn)
        {
            this.dbQuery = dbQuery;
            this.dbConn = dbConn;
        }

        internal void Fill(DataSet dbDataSet, string v)
        {
            throw new NotImplementedException();
        }
    }
}