using System;

namespace FbCopy.Tests
{
    class User : IEquatable<User>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Weight { get; set; }
        public decimal Rating { get; set; }
        public bool Active { get; set; }
        public DateTime Date { get; set; }
        public DateTime Day { get; set; }
        public decimal? LastRating { get; set; }

        public User()
        {
        }

        public User(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        public bool Equals(User other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && 
                string.Equals(Name, other.Name) &&
                Weight.Equals(other.Weight) &&
                Rating.Equals(other.Rating) && 
                Active == other.Active &&
                DatesAreEqual(Date, other.Date) &&
                DatesAreEqual(Day, other.Day) && 
                LastRating.Equals(other.LastRating);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((User) obj);
        }

        public static bool DatesAreEqual(DateTime d1, DateTime d2)
        {
            return Math.Abs(d1.Subtract(d2).TotalMilliseconds) < 1;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id;
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Weight.GetHashCode();
                hashCode = (hashCode * 397) ^ Rating.GetHashCode();
                hashCode = (hashCode * 397) ^ Active.GetHashCode();
                hashCode = (hashCode * 397) ^ Date.GetHashCode();
                hashCode = (hashCode * 397) ^ Day.GetHashCode();
                return hashCode;
            }
        }
    }
}