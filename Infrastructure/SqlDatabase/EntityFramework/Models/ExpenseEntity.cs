using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.SqlDatabase
{
    [Table("expenses")]
    public class ExpenseEntity
    {
        [Key, Column("id")]
        public int Id { get; set; }

        [Column("settled")]
        public bool Settled { get; set; }

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

        public ExpenseEntity()
        {
            Description = string.Empty;
            Currency = string.Empty;
        }
    }
}
