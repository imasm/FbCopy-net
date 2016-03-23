using System;
using System.Collections.Generic;
using FirebirdSql.Data.FirebirdClient;

namespace FbCopy.Firebird
{
    static class DatabaseExtensions
    {
        public static IEnumerable<T> Read<T>(this FbCommand cmd, Func<FbDataReader, T> func)
        {
            using (var dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    yield return func(dr);
                }
                dr.Close();
            }
        }

        public static string GetTrimmedString(this FbDataReader dr, int ordinal)
        {
            return dr.GetString(ordinal)?.Trim();
        }
    }
}
