using System;
using System.Collections.Generic;
using FbCopy.Firebird;

namespace FbCopy
{
    class DependencyBuilder
    {
        TableDependency _rootNode;
        private DbConnection _database;
        
        public TableDependency Build(DbConnection db, List<string> tables)
        {
            _database = db;
            _rootNode = new TableDependency("root");
            foreach (string tablename in tables)
            {
                if (!_rootNode.Contains(tablename))
                {
                    var node = new TableDependency(tablename);
                    _rootNode.Dependencies.Add(node);
                    FillTableDependecies(node);
                }
            }

            return _rootNode;
        }

        private void FillTableDependecies(TableDependency node)
        {
            List<string> deps = DbMetadata.GetTableDependencies(_database, node.TableName);
            foreach (string dep in deps)
            {
                if (dep.Equals(node.TableName, StringComparison.OrdinalIgnoreCase))
                {
                    Console.Error.WriteLine($"WARNING: self referencing table: {node.TableName}.");
                    continue;
                }

                if (!_rootNode.Contains(dep))
                {
                    TableDependency dependency = new TableDependency(dep);
                    node.Dependencies.Add(dependency);
                    FillTableDependecies(dependency);
                }
            }
        }
    }
}
