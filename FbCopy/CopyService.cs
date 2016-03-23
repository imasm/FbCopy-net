using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FbCopy.Firebird;
using FirebirdSql.Data.FirebirdClient;
using DbConnection = FbCopy.Firebird.DbConnection;

namespace FbCopy
{
    class CopyService: FbService
    {
        private readonly CopyOptions _options;
        private string _sourceCStr;
        private string _destCStr;
        private DbConnection _sourceDb;
        private DbConnection _destDb;

        private readonly List<CopyTableInfo> _tablesToCopy;
        private readonly List<CopyGeneratorInfo> _generatorsToCopy;
        

        public CopyService(CopyOptions opts)
        {
            this._options = opts;
            _tablesToCopy = new List<CopyTableInfo>();
            _generatorsToCopy = new List<CopyGeneratorInfo>();
        }

        public void Run(TextReader textReder)
        {
            PrepareConnectionStrins();
            SetupFromStdin(textReder);
            CopyTables();
            CopyGenerators();
        }

        private void PrepareConnectionStrins()
        {
            var sourceDbInfo = DatabaseInfo.Parse(_options.Source);
            var destDbInfo = DatabaseInfo.Parse(_options.Destination);

            if (string.IsNullOrEmpty(sourceDbInfo.Charset))
                UpdateCharset(sourceDbInfo);

            if (string.IsNullOrEmpty(destDbInfo.Charset))
                UpdateCharset(destDbInfo);

            _sourceCStr = sourceDbInfo.GetConnectionString();
            _destCStr = destDbInfo.GetConnectionString();
        }

        private void SetupFromStdin(TextReader textReder)
        {
            bool firstLine = true;
            bool newFormat = false;
            string line;
            string type = "T";

            _tablesToCopy.Clear();
            _generatorsToCopy.Clear();

            while ((line = textReder.ReadLine()) != null)
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
                    _generatorsToCopy.Add(new CopyGeneratorInfo(table, fields));
                    continue;
                }

                using (var db = new DbConnection(_sourceCStr))
                {
                    db.Open();
                    var primaryKeys = DbMetadata.GetPrimayKeys(db, table).Select(x => x.Quote()).ToArray();
                    _tablesToCopy.Add(new CopyTableInfo(table, fields.Split(','), @where, primaryKeys));
                }
            }
        }


        private void CopyTables()
        {
            using (_sourceDb = new DbConnection(_sourceCStr))
            {
                using (_destDb = new DbConnection(_destCStr))
                {
                    if (_sourceDb.Open() && _destDb.Open())
                    {
                        foreach (var copyTableInfo in _tablesToCopy)
                        {
                            CopyTable(copyTableInfo);
                            _destDb.CommitAndStartNewTransaction();
                        }
                    }
                    _destDb.Commit();
                }
            }
        }
        
        private void CopyTable(CopyTableInfo copyTableInfo)
        {
            WriteLine("Copy table " + copyTableInfo.TableName);

            string select = copyTableInfo.BuildSelectStatement();
            string updateOrInsert = copyTableInfo.BuildUpdateOrInsertStatement();
            string update = copyTableInfo.BuildUpdateStatement();
            string insert = copyTableInfo.BuildInsertStatement();

            if (_options.Verbose)
            {
                WriteLine(select);
                WriteLine(insert);
                WriteLine(update);
                WriteLine(updateOrInsert);
            }

            bool first = true;
            using (FbCommand cmd = _sourceDb.CreateCommand(select), insCmd = _destDb.CreateCommand(insert))
            {
                using (FbDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        int fieldCount = dr.FieldCount;
                        if (first)
                        {
                            for (int i = 0; i < fieldCount; i++)
                            {
                                insCmd.Parameters.Add(dr.GetName(i), dr[i]);
                            }
                            first = false;
                        }
                        else
                        {
                            for (int i = 0; i < fieldCount; i++)
                            {
                                insCmd.Parameters[i].Value = dr[i];
                            }
                        }

                        insCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private void CopyGenerators()
        {
            using (_sourceDb = new DbConnection(_sourceCStr))
            {
                using (_destDb = new DbConnection(_destCStr))
                {
                    if (_sourceDb.Open() && _destDb.Open())
                    {
                        foreach (var copyGenerator in _generatorsToCopy)
                        {
                            CopyGeneratorValues(copyGenerator);
                            _destDb.CommitAndStartNewTransaction();
                        }
                    }
                    _destDb.Commit();
                }
            }
        }


        private void CopyGeneratorValues(CopyGeneratorInfo copyGeneratorInfo)
        {
            WriteLine("Copy generator values " + copyGeneratorInfo.SourceName);
        }



        private void WriteLine(string message)
        {
            Console.WriteLine(message);
        }
    }
}
