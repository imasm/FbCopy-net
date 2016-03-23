using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FbCopy.Firebird;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Isql;
using NUnit.Framework;

namespace FbCopy.Tests
{
    public abstract class TestDbBase
    {
        protected virtual string DbFolder
        {
            get
            {
                string location = Assembly.GetExecutingAssembly().Location;

                UriBuilder uri = new UriBuilder(location);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        protected virtual string SourceDbPath
        {
            get { return Path.Combine(DbFolder, "dbsrc.fdb"); }
        }

        protected virtual string DestDbPath
        {
            get { return Path.Combine(DbFolder, "dbdest.fdb"); }
        }

        protected virtual int SourceDbDialect
        {
            get { return 3; }
        }

        protected virtual int DestDbDialect
        {
            get { return 3; }
        }

        protected virtual string SourceDbCharset
        {
            get { return "UTF8"; }
        }

        protected virtual string DestDbCharset
        {
            get { return "UTF8"; }
        }

        protected virtual void CreateSourceDb()
        {
            string connectionString = GetSourceConnectionString();
            FbConnection.CreateDatabase(connectionString, 4096, false, true);
            ExecuteScript(connectionString);
        }

        protected virtual void CreateDestDb()
        {
            string connectionString = GetDestConnectionString();
            FbConnection.CreateDatabase(connectionString, 4096, false, true);
            ExecuteScript(connectionString);
        }

        private void ExecuteScript(string connectionString)
        {
            using (var connection = new FbConnection(connectionString))
            {
                string script = ReadResourceScript();

                FbScript fbScript = new FbScript(script);
                fbScript.Parse();
                FbBatchExecution batch = new FbBatchExecution(connection);
                batch.AppendSqlStatements(fbScript);
                batch.Execute(true);
            }
        }

        private string ReadResourceScript()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "FbCopy.Tests.dbtest.sql";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        protected string GetSourceConnectionString()
        {
            return GetConnectionString(SourceDbPath, SourceDbCharset, SourceDbDialect);
        }

        protected string GetDestConnectionString()
        {
            return GetConnectionString(DestDbPath, DestDbCharset, DestDbDialect);
        }

        protected string GetConnectionString(string dbFile, string charset, int dialect)
        {
            FbConnectionStringBuilder csb = new FbConnectionStringBuilder();
            csb.Database = dbFile;
            csb.DataSource = "localhost";
            csb.UserID = "SYSDBA";
            csb.Password = "masterkey";
            csb.Dialect = SourceDbDialect;
            csb.Charset = SourceDbCharset;
            return csb.ConnectionString;
        }

        
    }
}
