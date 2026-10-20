namespace Core.Entities
{
    public class ExpenseShare
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public User Participant { get; set; }
        public Expense Expense { get; set; }

        public ExpenseShare()
        {
            Participant = new User();
            Expense = new Expense();
        }
    }
}
