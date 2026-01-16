using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.DataSecure
{
    public class ConnectionStringProvider
    {
        public string ConnectionString { get; }

        public ConnectionStringProvider(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }
}
