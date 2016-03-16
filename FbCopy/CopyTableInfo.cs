using System.Linq;
using System.Text;

namespace FbCopy
{
    internal class CopyTableInfo
    {
        public string TableName { get; }
        public string[] Fields { get; }
        public string Where { get; }
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
            return $"INSERT INTO {TableName} ({JoinList(Fields)}) VALUES ({GetParams(Fields)})";
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
                sb.Append($"{field}=?");
            }

            sb.Append(" WHERE ");

            first = true;
            foreach (var field in PrimaryKeys)
            {
                if (!first)
                    sb.Append(" AND ");

                first = false;
                sb.Append($"({field}=?)");
            }
            return sb.ToString();
        }

        public string BuildUpdateOrInsertStatement()
        {
            return $"UPDATE OR INSERT INTO {TableName} ({JoinList(Fields)})" +
                $" VALUES ({GetParams(Fields)})" +
                $" MATCHING ({JoinList(PrimaryKeys)})";
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
}
