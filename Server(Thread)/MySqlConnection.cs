using System;

namespace Server_Thread_
{
    internal class MySqlConnection
    {
        private string strDBConn;

        public MySqlConnection(string strDBConn)
        {
            this.strDBConn = strDBConn;
        }

        internal void Open()
        {
            throw new NotImplementedException();
        }
    }
}