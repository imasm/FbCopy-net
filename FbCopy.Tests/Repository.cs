
using FbCopy.Firebird;

namespace FbCopy.Tests
{
    abstract class Repository
    {
        protected string ConnectionString { get;}

        protected Repository(string connectionString)
        {
            ConnectionString = connectionString;
        }

        protected DbConnection NewConnection()
        {
            return new DbConnection(ConnectionString);
        }

        protected static string Sql(System.FormattableString formattable)
        {
            return formattable.ToString(new SqlFormatProvider());
        }


    }
}
