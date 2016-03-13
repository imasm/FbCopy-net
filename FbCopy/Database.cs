using FirebirdSql.Data.FirebirdClient;
using System;
using System.Data;

namespace FbCopy
{
    class Database : IDisposable
    {
        private FbConnection _connection;

        public string ConnectionString { get; private set; }
        public Database() { _connection = null; }

        public void Connect(DatabaseInfo dbInfo)
        {
            CreateConnectionString(dbInfo);

            Console.Error.WriteLine($"Connecting to: {dbInfo.Hostname}:{dbInfo.Database} as {dbInfo.Username}...");
            
            _connection = new FbConnection(ConnectionString);
            _connection.Open();

            Console.Error.WriteLine("Connnected.");

            if (string.IsNullOrEmpty(dbInfo.Charset))
            {                
                string charset = GetCharset();
                if (!string.IsNullOrEmpty(charset))
                {
                    Console.Error.WriteLine(charset);
                    if (string.Equals(charset, "NONE"))
                    {
                        Console.Error.WriteLine("No need for reconnecting.");
                        return;
                    }

                    Disconnect();
                    dbInfo.Charset = charset;
                    Connect(dbInfo);
                }
            }
        }

        public void Disconnect()
        {
            if (_connection != null)
            {
                Console.WriteLine("Disconnecting...");
                _connection.Close();
                _connection.Dispose();
                _connection = null;
                Console.WriteLine("Ok.");
            }
        }

        private void CreateConnectionString(DatabaseInfo dbInfo)
        {
            FbConnectionStringBuilder builder = new FbConnectionStringBuilder();
            builder.UserID = dbInfo.Username;
            builder.Password = dbInfo.Password;
            builder.DataSource = dbInfo.Hostname;
            builder.Database = dbInfo.Database;

            if (string.IsNullOrEmpty(dbInfo.Charset))
                builder.Charset = dbInfo.Charset;
            ConnectionString = builder.ToString();
        }

        private string GetCharset()
        {
            Console.Error.WriteLine("Reading charset: ");

            string charset = null;
            using (var trans = _connection.BeginTransaction())
            {
                using (var cmd = new FbCommand("SELECT rdb$character_set_name FROM rdb$database", _connection, trans))
                {
                    using (var dr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (dr.Read())
                        {
                            if (!dr.IsDBNull(0))
                                charset = dr.GetString(0).Trim();
                        }
                        dr.Close();
                    }
                }
            }
            return charset;
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }
    }
}
