using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.SqlDatabase
{
    [Table("users"),
       Index(nameof(UniqueKey), IsUnique = true),
       Index(nameof(Email), IsUnique = true),
       Index(nameof(Phone), IsUnique = true)]
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
        [Column("country_code"), MaxLength(8)]
        public string CountryCode { get; set; }

        [Column("unique_key")]
        public Guid UniqueKey { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        public PasswordEntity? Password { get; set; }

        public UserEntity()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            Email = string.Empty;
            Phone = string.Empty;
            CountryCode = string.Empty;
            // Default to true
            Active = true;
        }
    }
}
