using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UI.Models
{
    public class ExpenseViewModel
    {
        [Required]
        public string Description { get; set; }

        [Range(typeof(decimal), "0.01", "9999999999999999.99")]
        public decimal Amount { get; set; }

        public Guid GroupKey { get; set; }
        public Guid PayerKey { get; set; }
        public Guid IdempotencyKey { get; set; }
        public List<Guid> ParticipantKeys { get; set; } = [];
        public List<ExpenseMemberOptionViewModel> Members { get; set; } = [];

        public ExpenseViewModel()
        {
            Description = string.Empty;
        }
    }
}
