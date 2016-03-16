using System;
using System.Linq;

namespace FbCopy
{
    public class CopyService
    {
        private readonly CopyOptions _options;
        private Database _sourceDb;
        private Database _destDb;

        

       

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
            WriteLine(copyTableInfo.BuildUpdateOrInsertStatement());

        }

        private void WriteLine(string message)
        {
            Console.Error.WriteLine(message);
        }
    }
}
