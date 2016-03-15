using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FbCopy
{
    class TableDependency : IEquatable<TableDependency>, IComparable<TableDependency>
    {
        public string TableName { get; }
        public List<TableDependency> Dependencies { get; set; }

        public TableDependency(string name)
        {
            TableName = name;
            Dependencies = new List<TableDependency>();
        }


        public bool Contains(string table)
        {
            if (this.TableName.Equals(table, StringComparison.OrdinalIgnoreCase))
                return true;

            foreach (TableDependency tableDependency in this.Dependencies)
            {
                if (tableDependency.Contains(table))
                    return true;
            }

            return false;
        }

        public void PrintTree()
        {
            this.PrintTree(0);
        }

        protected void PrintTree(int level)
        {
            string indent = "";
            if (level > 0)
                indent = new string(' ', level * 2);

            Debug.WriteLine($"{indent}{this.TableName}");
            foreach (TableDependency tableDependency in this.Dependencies)
                tableDependency.PrintTree(level + 1);
        }

        #region IEquatable

        public override bool Equals(object obj)
        {
            TableDependency p = obj as TableDependency;
            if (p == null)
                return false;

            return string.Equals(TableName, p.TableName, StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(TableDependency other)
        {
            return string.Equals(TableName, other.TableName, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return TableName?.GetHashCode() ?? 0;
        }

        #endregion

        #region IComparable

        public int CompareTo(TableDependency other)
        {
            return string.CompareOrdinal(this.TableName, other?.TableName);
        }

        #endregion
    }
}
