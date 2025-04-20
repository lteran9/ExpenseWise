using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.SqlDatabase
{
   [Table("passwords"), Index(nameof(UserId), IsUnique = true)]
   public class PasswordEntity
   {
      [Key]
      public int Id { get; set; }

      [Column("user_id")]
      public int UserId { get; set; }

      [Column("cipher"), MaxLength(256)]
      public string Cipher { get; set; }
      [Column("encrypted"), MaxLength(512)]
      public string Encrypted { get; set; }

      [Column("updated_at")]
      public DateTime UpdatedAt { get; set; }

      public UserEntity User { get; set; } = null!;

      public PasswordEntity()
      {
         Cipher = string.Empty;
         Encrypted = string.Empty;
      }
   }
}