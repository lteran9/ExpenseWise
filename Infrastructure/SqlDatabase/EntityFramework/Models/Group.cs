using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseWise.Infrastructure.Sql
{
   [Table("groups")]
   public class Group
   {
      [Key, Column("id")]
      public int Id { get; set; }
      [Column("owner_id"), ForeignKey(nameof(User))]
      public int OwnerId { get; set; }

      [Column("name")]
      public string Name { get; set; }

      public Guid UniqueKey { get; set; }

      [Column("created_at")]
      public DateTime CreatedAt { get; set; }
      [Column("updated_at")]
      public DateTime UpdatedAt { get; set; }

      public Group()
      {
         Name = string.Empty;
      }
   }
}