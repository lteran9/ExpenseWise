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
      [Column("owner_id")]
      public int OwnerId { get; set; }

      [Column("name"), MaxLength(64)]
      public string Name { get; set; }

      [Column("unique_key")]
      public Guid UniqueKey { get; set; }

      [Column("created_at")]
      public DateTime CreatedAt { get; set; }
      [Column("updated_at")]
      public DateTime UpdatedAt { get; set; }

      [ForeignKey(nameof(OwnerId))]
      public User? Owner { get; set; }

      public Group()
      {
         Name = string.Empty;
      }
   }
}