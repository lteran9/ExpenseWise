using System;
using System.Globalization;

namespace Core.Entities
{
    /// <summary>
    /// Net balances for one group: expenses paid minus shares allocated, plus repayments sent minus repayments received.
    /// Voided expenses are excluded. A positive balance means the member is owed money.
    /// </summary>
    public class GroupLedger
    {
        private readonly Dictionary<Guid, decimal> _netByMember = new();

        public GroupLedger(IEnumerable<User> members, IEnumerable<Expense> expenses, IEnumerable<Repayment> repayments)
        {
            foreach (var expense in expenses.Where(expense => !expense.Voided))
            {
                TotalSpent += expense.Amount;
                if (expense.Payer != null)
                {
                    AddToBalance(expense.Payer.UniqueKey, expense.Amount);
                }

                foreach (var share in expense.Shares)
                {
                    AddToBalance(share.Participant.UniqueKey, -share.Amount);
                }
            }

            foreach (var repayment in repayments)
            {
                if (repayment.Sender != null)
                {
                    AddToBalance(repayment.Sender.UniqueKey, repayment.Amount);
                }

                if (repayment.Recipient != null)
                {
                    AddToBalance(repayment.Recipient.UniqueKey, -repayment.Amount);
                }
            }

            Balances = members
                .Select(member => new MemberBalance(member, BalanceOf(member.UniqueKey)))
                .ToList();
            Outstanding = Balances.Where(balance => balance.NetBalance > 0).Sum(balance => balance.NetBalance);
        }

        public decimal TotalSpent { get; }

        public decimal Outstanding { get; }

        public IReadOnlyList<MemberBalance> Balances { get; }

        public decimal BalanceOf(Guid memberKey) => _netByMember.GetValueOrDefault(memberKey);

        /// <summary>
        /// Matches the largest debtor with the largest creditor until every balance is settled.
        /// Ties are broken by display name, then by unique key, so the same ledger always yields the same suggestions.
        /// </summary>
        public IReadOnlyList<RepaymentSuggestion> SuggestRepayments()
        {
            var debtors = Positions(Balances.Where(balance => balance.NetBalance < 0).Select(balance => (balance.Member, -balance.NetBalance)));
            var creditors = Positions(Balances.Where(balance => balance.NetBalance > 0).Select(balance => (balance.Member, balance.NetBalance)));
            var suggestions = new List<RepaymentSuggestion>();

            static Position? LargestRemaining(IEnumerable<Position> positions) => positions
                .Where(position => position.Remaining > 0)
                .OrderByDescending(position => position.Remaining)
                .ThenBy(position => position.Member.Name, StringComparer.Ordinal)
                .ThenBy(position => position.Member.UniqueKey)
                .FirstOrDefault();

            while (LargestRemaining(debtors) is { } debtor &&
                   LargestRemaining(creditors) is { } creditor)
            {
                var amount = Math.Min(debtor.Remaining, creditor.Remaining);
                suggestions.Add(new RepaymentSuggestion(debtor.Member, creditor.Member, amount));
                debtor.Remaining -= amount;
                creditor.Remaining -= amount;
            }

            return suggestions;
        }

        /// <summary>
        /// Returns the reason a repayment cannot be recorded against this ledger state, or null when it is valid.
        /// </summary>
        public string? ValidateRepayment(Guid senderKey, Guid recipientKey, decimal amount)
        {
            if (amount <= 0)
            {
                return "Repayment amount must be greater than zero.";
            }

            if (decimal.Round(amount, 2) != amount)
            {
                return "Repayment amount can have at most two decimal places.";
            }

            if (senderKey == recipientKey)
            {
                return "The sender and recipient must be different members.";
            }

            var debt = Math.Max(-BalanceOf(senderKey), 0);
            if (debt == 0)
            {
                return "The sender does not owe money in this group.";
            }

            if (amount > debt)
            {
                return $"The amount exceeds the sender's current debt of {Format(debt)}.";
            }

            var credit = Math.Max(BalanceOf(recipientKey), 0);
            if (credit == 0)
            {
                return "The recipient is not owed money in this group.";
            }

            if (amount > credit)
            {
                return $"The amount exceeds the recipient's current credit of {Format(credit)}.";
            }

            return null;
        }

        private void AddToBalance(Guid memberKey, decimal amount)
        {
            _netByMember[memberKey] = _netByMember.GetValueOrDefault(memberKey) + amount;
        }

        private static List<Position> Positions(IEnumerable<(User Member, decimal Amount)> entries)
        {
            return entries
                .OrderByDescending(entry => entry.Amount)
                .ThenBy(entry => entry.Member.Name, StringComparer.Ordinal)
                .ThenBy(entry => entry.Member.UniqueKey)
                .Select(entry => new Position(entry.Member, entry.Amount))
                .ToList();
        }

        private static string Format(decimal amount) => amount.ToString("0.00", CultureInfo.InvariantCulture);

        private sealed class Position
        {
            public Position(User member, decimal remaining)
            {
                Member = member;
                Remaining = remaining;
            }

            public User Member { get; }
            public decimal Remaining { get; set; }
        }
    }

    public record MemberBalance(User Member, decimal NetBalance);

    public record RepaymentSuggestion(User From, User To, decimal Amount);
}
