using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.SqlDatabase
{
   [Table("users")]
   public class UserEntity
   {
      [Key, Column("id")]
      public int Id { get; set; }

      [Column("active")]
      public bool Active { get; set; }

      [Column("fist_name"), MaxLength(32)]
      public string FirstName { get; set; }
      [Column("last_name"), MaxLength(64)]
      public string LastName { get; set; }
      [Column("email"), MaxLength(128)]
      public string Email { get; set; }
      [Column("phone"), MaxLength(16)]
      public string Phone { get; set; }
      [Column("password"), MaxLength(256)]
      public string Password { get; set; }

      [Column("created_at")]
      public DateTime CreatedAt { get; set; }
      [Column("updated_at")]
      public DateTime UpdatedAt { get; set; }

      public UserEntity()
      {
         FirstName = string.Empty;
         LastName = string.Empty;
         Email = string.Empty;
         Phone = string.Empty;
         Password = string.Empty;
         // Default to true
         Active = true;
      }
   }
}