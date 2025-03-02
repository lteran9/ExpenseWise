using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseWise.Infrastructure.Sql
{
   [Table("member_of")]
   public class MemberOf
   {
      [Key]
      public int Id { get; set; }
      [Column("user_id")]
      public int UserId { get; set; }
      [Column("group_id")]
      public int GroupId { get; set; }

      [Column("created_at")]
      public DateTime CreatedAt { get; set; }
      [Column("updated_at")]
      public DateTime UpdatedAt { get; set; }

      [ForeignKey(nameof(UserId))]
      public User? User { get; set; }
      [ForeignKey(nameof(GroupId))]
      public Group? Group { get; set; }
   }
}