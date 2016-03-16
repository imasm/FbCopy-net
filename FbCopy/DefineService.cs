using System;
using System.Linq;
using System.Collections.Generic;

namespace FbCopy
{
    public class DefineService
    {
        private readonly DefineOptions _options;

        private List<string> _tables;
        private TableDependency _dependencyTree;
        private Database _sourceDb;
        private Database _destDb;


        public DefineService(DefineOptions opts)
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

                    LoadTables();
                    LoadDependencyTree();

                    BuildOutput();

                    _sourceDb.Disconnect();
                    _destDb.Disconnect();
                }
            }
        }

        private void LoadTables()
        {
            _tables = _sourceDb.GetListOfTables();
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
            var srcfields = _sourceDb.GetFields(tablename);
            var destfields = _destDb.GetFields(tablename);

            var fields = srcfields.Intersect(destfields).Select(x => x.Quote());
            var missing = srcfields.Except(destfields).Select(x => x.Quote());
            var extra = destfields.Except(srcfields).Select(x => x.Quote());

            Output($"#T:{tablename}:{string.Join(",", fields)}:{string.Join(",", missing)}:{string.Join(",", extra)}:");
        }

        private void CompareGenerators()
        {
            var srcGenerators = _sourceDb.GetListOfGenerators();
            var destGenerators = _destDb.GetListOfGenerators();

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
