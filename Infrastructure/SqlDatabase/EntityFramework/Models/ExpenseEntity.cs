using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.SqlDatabase
{
    [Table("expenses"), Index(nameof(UniqueKey), IsUnique = true)]
    public class ExpenseEntity
    {
        [Key, Column("id")]
        public int Id { get; set; }
        [Column("group_id")]
        public int GroupId { get; set; }
        [Column("created_by")]
        public int UserId { get; set; }

        [Column("description")]
        public string Description { get; set; }
        [Column("currency")]
        public string Currency { get; set; }

        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("unique_key")]
        public Guid UniqueKey { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [ForeignKey(nameof(GroupId))]
        public GroupEntity? Group { get; set; }
        [ForeignKey(nameof(UserId))]
        public UserEntity? CreatedBy { get; set; }

        public ExpenseEntity()
        {
            Description = string.Empty;
            Currency = string.Empty;
        }
    }
}
