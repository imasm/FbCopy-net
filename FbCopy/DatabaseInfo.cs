using System;

namespace FbCopy
{
    class DatabaseInfo
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public string Hostname { get; set; }
        public string Charset { get; set; }

        public static DatabaseInfo Parse(string s)
        {
            DatabaseInfo dbInfo = new DatabaseInfo();

            dbInfo.Charset = "";
            dbInfo.Hostname = "";

            int pos = s.IndexOf("@");

            if (pos == -1)
            {
                string usr = GetEnv("ISC_USER");
                string pwd = GetEnv("ISC_PASSWORD");
                if (string.IsNullOrEmpty(usr) || string.IsNullOrEmpty(pwd))
                {
                    throw new Exception("Missing @ in path and ISC_USER and ISC_PASSWORD not set.");
                }

                dbInfo.Username = usr;
                dbInfo.Password = pwd;
            }
            else
            {
                string left = s.Substring(0, pos);
                s= s.Remove(0, pos + 1);
                pos = left.IndexOf(":");
                if (pos == -1)
                {
                    throw new Exception("Missing : in username:password part");
                }

                dbInfo.Username = left.Substring(0, pos);
                dbInfo.Password = left.Substring(pos + 1);
            }

            pos = s.IndexOf(":");
            if (pos != -1)
            {
                if (pos > 1)  // to avoid drive letter
                {
                    dbInfo.Hostname = s.Substring(0, pos);
                    s=s.Remove(0, pos + 1);
                }
            }

            pos = s.IndexOf("?");
            if (pos != -1)
            {
                dbInfo.Charset = s.Substring(pos + 1);
                s=s.Remove(pos);
            }
            dbInfo.Database = s;

            return dbInfo;
        }

        private static string GetEnv(string variable)
        {
            return Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.User)
                ?? Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.Machine);
        }
    };
}
