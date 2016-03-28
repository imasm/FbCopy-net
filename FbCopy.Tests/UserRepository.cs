using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using FirebirdSql.Data.FirebirdClient;

namespace FbCopy.Tests
{
    class UserRepository:Repository
    {
        static string[] Fields = new string[] { "id", "name", "weight", "rating", "active", "ddate", "dday", "last_rating", "description" };

        public UserRepository(string connectionString): base(connectionString)
        {
        }

        public void InsertUsers(IEnumerable<User> users)
        {
           
            using (var connection = NewConnection())
            {
                connection.Open();
                
                string fields = string.Join(",", Fields);
                string args = string.Join(",", Fields.Select(x => '?'));


                    string sql = $"INSERT INTO USERS ({fields}) VALUES ({args})";

                using (FbCommand cmd = connection.CreateCommand(sql))
                {
                    cmd.Parameters.Add(new FbParameter("@id", FbDbType.Integer));
                    cmd.Parameters.Add(new FbParameter("@name", FbDbType.VarChar));
                    cmd.Parameters.Add(new FbParameter("@weight", FbDbType.Double));
                    cmd.Parameters.Add(new FbParameter("@rating", FbDbType.Numeric));
                    cmd.Parameters.Add(new FbParameter("@active", FbDbType.SmallInt));
                    cmd.Parameters.Add(new FbParameter("@ddate", FbDbType.TimeStamp));
                    cmd.Parameters.Add(new FbParameter("@dday", FbDbType.Date));
                    cmd.Parameters.Add(new FbParameter("@last_rating", FbDbType.Numeric));
                    cmd.Parameters.Add(new FbParameter("@description", FbDbType.Text));
                    cmd.Prepare();


                    foreach (var user in users)
                    {
                        cmd.Parameters[0].Value = user.Id;
                        cmd.Parameters[1].Value = user.Name;
                        cmd.Parameters[2].Value = user.Weight;
                        cmd.Parameters[3].Value = user.Rating;
                        cmd.Parameters[4].Value = user.Active;
                        cmd.Parameters[5].Value = user.Date;
                        cmd.Parameters[6].Value = user.Day;
                        cmd.Parameters[7].Value = user.LastRating;
                        cmd.Parameters[8].Value = user.Description;

                        cmd.ExecuteNonQuery();
                    }
                }
                connection.Commit();
            }
        }

        public List<User> GetAllUsers()
        {
            List<User> list = new List<User>();

            using (var connection = NewConnection())
            {
                string fields = string.Join(",", Fields);
                connection.Open();
                using (var cmd = connection.CreateCommand($"select {fields} from USERS order by id"))
                {
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            int id = Convert.ToInt32(dr[0]);
                            string name = Convert.ToString(dr[1]);                            
                            User user = new User(id, name);
                            user.Weight = Convert.ToDouble(dr[2]);
                            user.Rating = Convert.ToDecimal(dr[3]);
                            user.Active = Convert.ToBoolean(dr[4]);
                            user.Date = Convert.ToDateTime(dr[5]);
                            user.Day = Convert.ToDateTime(dr[6]);
                            if (dr.IsDBNull(7))
                                user.LastRating = null;
                            else
                                user.LastRating = Convert.ToDecimal(dr[7]);
                            user.Description = Convert.ToString(dr[8]);
                            list.Add(user);
                        }
                    }
                }

            }
            return list;
        }

        public List<User> CreateFakes(int num)
        {
            int ids = 0;
            var testUsers = new Faker<User>()
                    .StrictMode(true)
                    .RuleFor(o => o.Id, f => ids++)
                    .RuleFor(o => o.Name, f => f.Name.FirstName())
                    .RuleFor(o => o.Weight, f => Math.Round(f.Random.Double() * 100, 3))
                    .RuleFor(o => o.Rating, f => Math.Round((decimal)f.Random.Double(), 3))
                    .RuleFor(o => o.Active, f => f.Random.Bool())
                    .RuleFor(o => o.Date, f => f.Date.Between(new DateTime(2010, 1, 1), DateTime.Today))
                    .RuleFor(o => o.Day, f => f.Date.Between(new DateTime(2010, 1, 1), DateTime.Today).Date)
                    .RuleFor(o => o.LastRating, f => f.Random.Bool() ? null : (decimal?)0.1)
                    .RuleFor(o => o.Description, f => f.Lorem.Lines());

            return testUsers.Generate(num).ToList();
        }
    }
}
