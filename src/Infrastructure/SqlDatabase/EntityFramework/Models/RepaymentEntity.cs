using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.SqlDatabase
{
    [Table("repayments"), Index(nameof(UniqueKey), IsUnique = true), Index(nameof(GroupId))]
    public class RepaymentEntity
    {
        [Key, Column("id")]
        public int Id { get; set; }
        [Column("group_id"), ForeignKey(nameof(Group))]
        public int GroupId { get; set; }
        [Column("sender_id"), ForeignKey(nameof(Sender))]
        public int SenderUserId { get; set; }
        [Column("recipient_id"), ForeignKey(nameof(Recipient))]
        public int RecipientUserId { get; set; }
        [Column("recorded_by"), ForeignKey(nameof(RecordedBy))]
        public int RecordedByUserId { get; set; }

        [Column("amount", TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        [Column("currency")]
        public string Currency { get; set; }

        [Column("unique_key")]
        public Guid UniqueKey { get; set; }
        [Column("occurred_at", TypeName = "datetime(6)")]
        public DateTime OccurredAt { get; set; }
        [Column("created_at", TypeName = "datetime(6)")]
        public DateTime CreatedAt { get; set; }

        public GroupEntity? Group { get; set; }
        public UserEntity? Sender { get; set; }
        public UserEntity? Recipient { get; set; }
        public UserEntity? RecordedBy { get; set; }

        public RepaymentEntity()
        {
            Currency = string.Empty;
        }
    }
}
