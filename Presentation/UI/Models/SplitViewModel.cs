using System;

namespace UI.Models
{
    public class SplitVieWModel
    {
        public int GroupMemberCount { get; set; }

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