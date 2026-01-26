using System;

namespace Core.Entities
{
    public class Split
    {
        public int Id { get; set; }

        public bool Paid { get; set; }

        public DateTime? PaidOn { get; set; }

        public User User { get; set; }
        public Group Group { get; set; }
        public Expense Expense { get; set; }

        public Split()
        {
            User = new User();
            Group = new Group();
            Expense = new Expense();
        }
    }
}
