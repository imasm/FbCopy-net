using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;

namespace FbCopy.Tests
{
    class UserRepository:Repository
    {
        const string Fields = "id, name, weight, rating, active, ddate, dday, last_rating";

        public UserRepository(string connectionString): base(connectionString)
        {
        }

        public void InsertUsers(IEnumerable<User> users)
        {
           
            using (var connection = NewConnection())
            {
                connection.Open();
                foreach (var user in users)
                {
                    string args = Sql($"{user.Id}, ") +
                        Sql($"{user.Name}, ") +
                        Sql($"{user.Weight}, ") +
                        Sql($"{user.Rating}, ") +
                        Sql($"{user.Active}, ") +
                        Sql($"{ user.Date}, ") +
                        Sql($"{user.Day}, ") +
                        Sql($"{user.LastRating}");

                    string sql = $"INSERT INTO USERS ({Fields}) VALUES ({args})";
                    
                    connection.Execute(sql);
                }

                connection.Commit();
            }
        }

        public List<User> GetAllUsers()
        {
            List<User> list = new List<User>();

            using (var connection = NewConnection())
            {
                connection.Open();
                using (var cmd = connection.CreateCommand($"select {Fields} from USERS order by id"))
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
                    .RuleFor(o => o.LastRating, f => f.Random.Bool() ? null : (decimal?)0.1);

            return testUsers.Generate(num).ToList();
        }
    }
}
