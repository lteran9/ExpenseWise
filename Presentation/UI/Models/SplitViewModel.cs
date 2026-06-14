using System;
using System.ComponentModel.DataAnnotations;

namespace UI.Models
{
    public class SplitViewModel
    {
        public int GroupMemberCount { get; set; }

        public Guid GroupKey { get; set; }

        public required IEnumerable<ExpenseViewModel> ExpenseList { get; set; }

        public string GetMyShare()
        {
            if (GroupMemberCount > 0)
            {
                return (ExpenseList?.Sum(x => x.Amount) / GroupMemberCount)?.ToString("C") ?? "$0.00";
            }

            return "$0.00";
        }
    }
}