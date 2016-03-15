using System;
using System.Collections.Generic;

namespace FbCopy
{
    class DependencyBuilder
    {
        TableDependency rootNode;
        private Database database;

        public TableDependency Build(Database db, List<string> tables)
        {
            database = db;
            rootNode = new TableDependency("root");
            foreach (string tablename in tables)
            {
                if (!rootNode.Contains(tablename))
                {
                    var node = new TableDependency(tablename);
                    rootNode.Dependencies.Add(node);
                    FillTableDependecies(node);
                }
            }

            return rootNode;
        }

        private void FillTableDependecies(TableDependency node)
        {
            List<string> deps = database.GetTableDependencies(node.TableName);
            foreach (string dep in deps)
            {
                if (dep.Equals(node.TableName, StringComparison.OrdinalIgnoreCase))
                {
                    Console.Error.WriteLine($"WARNING: self referencing table: {node.TableName}.");
                    continue;
                }

                if (!rootNode.Contains(dep))
                {
                    TableDependency dependency = new TableDependency(dep);
                    node.Dependencies.Add(dependency);
                    FillTableDependecies(dependency);
                }
            }
        }
    }
}
