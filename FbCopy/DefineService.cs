using System;
using System.Collections.Generic;
using System.Linq;
using FbCopy.Firebird;

namespace FbCopy
{
    class DefineService: FbService
    {
        private readonly DefineOptions _options;

        private List<string> _tables;
        private TableDependency _dependencyTree;
        
        private DbConnection _sourceDb;
        private DbConnection _destDb;

        public DefineService(DefineOptions opts)
        {
            this._options = opts;
        }

        public void Run()
        {
            var sourceDbInfo = DatabaseInfo.Parse(_options.Source);
            var destDbInfo = DatabaseInfo.Parse(_options.Source);

            if (string.IsNullOrEmpty(sourceDbInfo.Charset))
                UpdateCharset(sourceDbInfo);

            if (string.IsNullOrEmpty(destDbInfo.Charset))
                UpdateCharset(destDbInfo);

            var sourceCStr = sourceDbInfo.GetConnectionString();
            var destCStr = destDbInfo.GetConnectionString();

            using (_sourceDb = new DbConnection(sourceCStr))
            {
                using (_destDb = new DbConnection(destCStr))
                {
                    if (_sourceDb.Open() && _destDb.Open())
                    {
                        LoadTables();
                        LoadDependencyTree();
                        BuildOutput();
                    }
                }
            }
        }

        private void LoadTables()
        {
            _tables = DbMetadata.GetListOfTables(_sourceDb);
        }

        private void LoadDependencyTree()
        {
            DependencyBuilder dependencyBuilder = new DependencyBuilder();
            _dependencyTree = dependencyBuilder.Build(_sourceDb, _tables);
            _dependencyTree.PrintTree();
        }

        private void BuildOutput()
        {
            CompareTables(_dependencyTree);
            CompareGenerators();
        }

        private void CompareTables(TableDependency tableDependency)
        {
            foreach (var dependency in tableDependency.Dependencies)
                CompareTables(dependency);

            if (!tableDependency.TableName.Equals("root", StringComparison.OrdinalIgnoreCase))
                CompareFields(tableDependency.TableName);
        }

        private void CompareFields(string tablename)
        {
            var srcfields = DbMetadata.GetFields(_sourceDb, tablename);
            var destfields = DbMetadata.GetFields(_destDb, tablename);

            var fields = srcfields.Intersect(destfields).Select(x => x.Quote());
            var missing = srcfields.Except(destfields).Select(x => x.Quote());
            var extra = destfields.Except(srcfields).Select(x => x.Quote());

            Output($"#T:{tablename}:{string.Join(",", fields)}:{string.Join(",", missing)}:{string.Join(",", extra)}:");
        }

        private void CompareGenerators()
        {
            var srcGenerators = DbMetadata.GetListOfGenerators(_sourceDb);
            var destGenerators = DbMetadata.GetListOfGenerators(_destDb);

            foreach (var srcGenerator in srcGenerators)
            {
                bool existInDest = destGenerators.Contains(srcGenerator, StringComparer.OrdinalIgnoreCase);
                string destGenerator = existInDest ? srcGenerator : "";
                Output($"#G:{srcGenerator}:{destGenerator}:");
            }
        }

        private void Output(string message)
        {
            Console.WriteLine(message);
        }

       
    }
}
