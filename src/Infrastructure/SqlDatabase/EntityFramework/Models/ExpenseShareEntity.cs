using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.SqlDatabase
{
    [Table("expense_shares"), Index(nameof(ExpenseId), nameof(UserId), IsUnique = true)]
    public class ExpenseShareEntity
    {
        [Key, Column("id")]
        public int Id { get; set; }

        [Column("expense_id")]
        public int ExpenseId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("amount", TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(ExpenseId))]
        public ExpenseEntity? Expense { get; set; }

        [ForeignKey(nameof(UserId))]
        public UserEntity? Participant { get; set; }
    }
}
