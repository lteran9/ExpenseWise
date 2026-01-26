using System;

namespace Core.Entities
{
    public class Group
    {
        public int Id { get; set; }

        public bool Active { get; set; }

        public string Name { get; set; }

        public Guid UniqueKey { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public User Owner { get; set; }
        public List<User> Members { get; set; }

        public Group()
        {
            Name = string.Empty;
            Owner = new User();
            Members = new List<User>();
            Active = true;
        }
    }
}
