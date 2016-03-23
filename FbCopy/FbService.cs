using System;
using FbCopy.Firebird;

namespace FbCopy
{
    class FbService
    {
        protected bool UpdateCharset(DatabaseInfo dbInfo)
        {
            var connectionString = dbInfo.GetConnectionString();
            using (var db = new DbConnection(connectionString))
            {
                if (db.Open())
                {
                    dbInfo.Charset = DbMetadata.GetCharset(db);
                    db.Close();
                }
            }

            return !dbInfo.Charset.Equals("NONE", StringComparison.OrdinalIgnoreCase);
        }
    }
}
