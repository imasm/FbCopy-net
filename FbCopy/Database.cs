using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Linq;

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

            WriteLine($"Connecting to: {dbInfo.Hostname}:{dbInfo.Database} as {dbInfo.Username}...");
            
            _connection = new FbConnection(ConnectionString);
            _connection.Open();

            WriteLine("Connnected.");

            if (string.IsNullOrEmpty(dbInfo.Charset))
            {                
                string charset = GetCharset();
                if (!string.IsNullOrEmpty(charset))
                {
                    WriteLine(charset);
                    if (string.Equals(charset, "NONE"))
                    {
                        WriteLine("No need for reconnecting.");
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
                WriteLine("Disconnecting...");
                _connection.Close();
                _connection.Dispose();
                _connection = null;
                WriteLine("Ok.");
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
            const string sql = "SELECT rdb$character_set_name FROM rdb$database";
            WriteLine("Reading charset: ");
            return Execute(sql).FirstOrDefault();
        }

        public List<string> GetListOfTables()
        {
            const string sql = "select RDB$RELATION_NAME from RDB$RELATIONS " +
                               "where (RDB$SYSTEM_FLAG = 0 or RDB$SYSTEM_FLAG is null) " +
                               "and RDB$VIEW_SOURCE is null ORDER BY 1";

            WriteLine("Loading list of tables...");
            return Execute(sql);
        }

        public List<string> GetListOfGenerators()
        {
            const string sql = "select RDB$GENERATOR_NAME from RDB$GENERATORS " +
                               "where (RDB$SYSTEM_FLAG = 0 or RDB$SYSTEM_FLAG is null) order by 1";

            WriteLine("Loading list of generators ...");
            return Execute(sql);
        }

        public List<string> GetTableDependencies(string table)
        {
            const string sqlFk = "select r2.rdb$relation_name from rdb$relation_constraints r1" +
                                    " join rdb$ref_constraints c ON r1.rdb$constraint_name = c.rdb$constraint_name" +
                                    " join rdb$relation_constraints r2 on c.RDB$CONST_NAME_UQ  = r2.rdb$constraint_name" +
                                    " where r1.rdb$relation_name= @tableName " +
                                    " and (r1.rdb$constraint_type='FOREIGN KEY') ";

            const string sqlCheck = "select distinct d.RDB$DEPENDED_ON_NAME from rdb$relation_constraints r " +
                                        " join rdb$check_constraints c on r.rdb$constraint_name=c.rdb$constraint_name " +
                                        "      and r.rdb$constraint_type = 'CHECK' " +
                                        " join rdb$dependencies d on d.RDB$DEPENDENT_NAME = c.rdb$trigger_name and d.RDB$DEPENDED_ON_TYPE = 0 " +
                                        "      and d.rdb$DEPENDENT_TYPE = 2 and d.rdb$field_name is null " +
                                        " where r.rdb$relation_name= @tableName ";

            List<string> dependencies = new List<string>();
            using (var trans = _connection.BeginTransaction())
            {
                using (FbCommand cmdFk = new FbCommand(sqlFk, _connection, trans),
                    cmdCheck = new FbCommand(sqlCheck, _connection, trans))
                {
                    cmdFk.Parameters.Add("@tableName", FbDbType.VarChar).Value =table;
                    cmdCheck.Parameters.Add("@tableName", FbDbType.VarChar).Value = table;
                    
                    dependencies.AddRange(cmdFk.Read(x => x.GetTrimmedString(0)));
                    dependencies.AddRange(cmdCheck.Read(x => x.GetTrimmedString(0)));
                    dependencies.Sort();
                }
            }

            return dependencies;
        }

        public List<string> GetFields(string tablename)
        {
            const string sql = " SELECT r.rdb$field_name FROM rdb$relation_fields r" +
                               " JOIN rdb$fields f ON r.rdb$field_source = f.rdb$field_name" +
                               " WHERE r.rdb$relation_name = @tableName " +
                               " AND f.rdb$computed_blr is null" +
                               " ORDER BY 1";

            return Execute(sql, tablename);
        }

        public List<string> GetPrimayKeys(string tablename)
        {
            const string sql = " select i.rdb$field_name" +
                               " from rdb$relation_constraints r, rdb$index_segments i " +
                               " where r.rdb$relation_name=@tablename and r.rdb$index_name=i.rdb$index_name" +
                               " and (r.rdb$constraint_type='PRIMARY KEY') ";

            
            return Execute(sql, tablename);
        }

        private List<string> Execute(string sql)
        {
            List<string> list = new List<string>();
            using (var trans = _connection.BeginTransaction())
            {
                using (FbCommand cmd = new FbCommand(sql, _connection, trans))
                {
                    list.AddRange(cmd.Read(x => x.GetTrimmedString(0)));
                }
            }
            return list;
        }

        private List<string> Execute(string sql, string tablename)
        {
            List<string> list = new List<string>();
            using (var trans = _connection.BeginTransaction())
            {
                using (FbCommand cmd = new FbCommand(sql, _connection, trans))
                {
                    cmd.Parameters.Add("@tableName", FbDbType.VarChar).Value = tablename;
                    list.AddRange(cmd.Read(x => x.GetTrimmedString(0)));
                }
            }
            return list;
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

        private void WriteLine(string message)
        {
            Console.Error.WriteLine(message);
        }
    }
}
