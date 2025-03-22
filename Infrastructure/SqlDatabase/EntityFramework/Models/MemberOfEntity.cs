using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.SqlDatabase
{
   [Table("member_of"), Index(nameof(UserId), nameof(GroupId), IsUnique = true)]
   public class MemberOfEntity
   {
      [Key]
      public int Id { get; set; }
      [Column("user_id")]
      public int? UserId { get; set; }
      [Column("group_id")]
      public int? GroupId { get; set; }

      [Column("active")]
      public bool Active { get; set; }

      [Column("created_at")]
      public DateTime CreatedAt { get; set; }
      [Column("updated_at")]
      public DateTime UpdatedAt { get; set; }

      [ForeignKey(nameof(UserId))]
      public UserEntity? User { get; set; }
      [ForeignKey(nameof(GroupId))]
      public GroupEntity? Group { get; set; }

      public MemberOfEntity()
      {
         // Default to true
         Active = true;
      }
   }
}