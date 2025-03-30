using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.SqlDatabase
{
   [Table("splits")]
   public class SplitEntity
   {
      [Key, Column("id")]
      public int Id { get; set; }
      [Column("user_id")]
      public int UserId { get; set; }
      [Column("group_id")]
      public int GroupId { get; set; }
      [Column("expense_id")]
      public int ExpenseId { get; set; }

      [Column("created_at")]
      public DateTime CreatedAt { get; set; }
      [Column("updated_at")]
      public DateTime UpdatedAt { get; set; }

      [ForeignKey(nameof(UserId))]
      public UserEntity? User { get; set; }
      [ForeignKey(nameof(GroupId))]
      public GroupEntity? Group { get; set; }
      [ForeignKey(nameof(ExpenseId))]
      public ExpenseEntity? Expense { get; set; }
   }
}