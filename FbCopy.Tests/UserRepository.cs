using System;
using System.Collections.Generic;
using FbCopy.Firebird;

namespace FbCopy.Tests
{
    class UserRepository
    {
        private readonly string _connectionString;
        
        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void InsertUsers(IEnumerable<User> users)
        {
            using (var connection = new DbConnection(_connectionString))
            {
                connection.Open();
                foreach (var user in users)
                    connection.Execute($"INSERT INTO USERS (id, name) VALUES ({user.Id}, '{user.Name}')");

                connection.Commit();
            }
        }

        public List<User> GetAllUsers()
        {
            List<User> list = new List<User>();

            using (var connection = new DbConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand("select id, name from USERS order by id"))
                {
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            int id = Convert.ToInt32(dr[0]);
                            string name = Convert.ToString(dr[1]);
                            User user = new User(id, name);
                            list.Add(user);
                        }
                    }
                }

            }
            return list;
        }
    }
}
