using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;

namespace FbCopy
{
    public class CopyService
    {
        private readonly CopyOptions _options;
        private Database _sourceDb;
        private Database _destDb;

        private class CopyGeneratorInfo
        {
            public string SourceName { get;  }
            public string DestName { get;  }

            public CopyGeneratorInfo(string sourceName, string destName)
            {
                SourceName = sourceName;
                DestName = destName;
            }
        }

        private class CopyTableInfo
        {
            public string TableName { get;  }
            public string[] Fields { get;  }
            public string Where { get;  }
            public string[] PrimaryKeys { get; }

            public CopyTableInfo(string tableName, string[] fields, string @where, string[] primaryKeys)
            {
                TableName = tableName;
                Fields = fields;
                Where = @where;
                PrimaryKeys = primaryKeys;
            }

            public string BuildSelectStatement()
            {
                string select = $"SELECT {JoinList(Fields)} FROM {TableName}";
                if (!string.IsNullOrEmpty(this.Where))
                    select = select + " " + this.Where;

                return select;
            }

            public string BuildInsertStatement()
            {
                return $"INSERT INTO {TableName} ({JoinList(Fields)}) VALUES({GetParams(Fields)})";
            }

            public string BuildUpdateStatement()
            {
                bool first = true;
                StringBuilder sb = new StringBuilder($"UPDATE {TableName} SET ");
                foreach (var field in Fields)
                {
                    if (!first)
                        sb.Append(",");

                    first = false;
                    sb.Append($"{field} =  ?");
                }

                sb.Append(" WHERE ");

                foreach (var field in PrimaryKeys)
                {
                    if (!first)
                        sb.Append(" AND ");

                    first = false;
                    sb.Append($"({field} =  ?)");
                }
                return sb.ToString();
            }

            private static string JoinList(string[] items)
            {
                return string.Join(",", items);
            }

            private static string GetParams(string[] fields)
            {
                return string.Join(",", fields.Select(x => "?"));
            }
        }

        public CopyService(CopyOptions opts)
        {
            this._options = opts;
        }

        public void Run()
        {
            var sourceDbInfo = DatabaseInfo.Parse(_options.Source);
            var destDbInfo = DatabaseInfo.Parse(_options.Source);

            using (_sourceDb = new Database())
            {
                using (_destDb = new Database())
                {
                    _sourceDb.Connect(sourceDbInfo);
                    _destDb.Connect(destDbInfo);

                    SetupFromStdin();

                    _sourceDb.Disconnect();
                    _destDb.Disconnect();
                }
            }
        }
       

        private void SetupFromStdin()
        {
            bool firstLine = true;
            bool newFormat = false;
            string line;
            string type = "T";

            while ((line = ReadLine()) != null)
            {
                int pos = line.IndexOf(":", StringComparison.OrdinalIgnoreCase);
                if (pos < 0)
                    WriteLine("Received line without colon, ignoring.");


                if (firstLine)
                {
                    newFormat = (pos == 2) && line.StartsWith("#");
                    WriteLine($"{(newFormat ? "New" : "Old")} format detected.");
                    firstLine = false;
                }

                if (newFormat)
                {
                    type = line.Substring(1, 1);

                    line = line.Substring(3);
                    pos = line.IndexOf(":", StringComparison.OrdinalIgnoreCase);
                    if (pos < 0)
                    {
                        WriteLine("Received line without object name, ignoring.");
                        continue;
                    }
                }

                string table = line.Substring(0, pos);
                line = line.Remove(0, pos + 1);
                
                pos = line.IndexOf(":", StringComparison.OrdinalIgnoreCase);

                if (pos < 0)
                {
                    WriteLine($"{(type == "G" ? "Generator" : "Column list")} for {table} not terminated, ignoring.");
                    continue;
                }
                
                string fields = line.Substring(0, pos);
                line = line.Remove(0, pos + 1);

                pos = line.LastIndexOf(":", StringComparison.OrdinalIgnoreCase);    // search for optional where clause
                string @where = " ";
                if ((pos > 0) && (line.Length >= pos))
                    @where = line.Substring(pos + 1);

                if (string.IsNullOrEmpty(fields))
                {
                    WriteLine($"Object {table} does not exist in destination db, skipping.");
                    continue;
                }

                if (newFormat && type.Equals("G", StringComparison.OrdinalIgnoreCase))    // generator
                {
                    CopyGeneratorValues(new CopyGeneratorInfo(table, fields));
                    continue;
                }

                var primaryKeys = _sourceDb.GetPrimayKeys(table).Select( x => x.Quote()).ToArray();
                CopyTable(new CopyTableInfo(table, fields.Split(','), @where, primaryKeys));
            }
        }

        private string ReadLine()
        {
            return Console.ReadLine();
        }
        
        private void CopyGeneratorValues(CopyGeneratorInfo copyGeneratorInfo)
        {
            WriteLine("Copy generator values " + copyGeneratorInfo.SourceName);
        }

        private void CopyTable(CopyTableInfo copyTableInfo)
        {
            WriteLine("Copy table " + copyTableInfo.TableName);
            WriteLine(copyTableInfo.BuildSelectStatement());
            WriteLine(copyTableInfo.BuildInsertStatement());
            WriteLine(copyTableInfo.BuildUpdateStatement());
        }

        private void WriteLine(string message)
        {
            Console.Error.WriteLine(message);
        }
    }
}
