using System;
using Application.UseCases.FluentValidation;
using Application.UseCases.MediatR;
using Application.UseCases.Ports;
using Core.Entities;
using FluentValidation;
using MediatR;

namespace Application.UseCases
{
    public class CreateExpense : BaseRequestHandler<CreateExpenseRequest, CreateExpenseResponse>
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly AbstractValidator<CreateExpenseRequest> _validator;

        public CreateExpense(IExpenseRepository expenseRepository, IGroupRepository groupRepository)
        {
            _expenseRepository = expenseRepository;
            _groupRepository = groupRepository;
            _validator = new CreateExpenseRequestValidator();
        }

        public override async Task<ResponseWrapper<CreateExpenseResponse>> Handle(CreateExpenseRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (validationResult.IsValid)
            {
                var group = await _groupRepository.FindByUniqueKeyAsync(request.GroupKey);
                if (group == null)
                {
                    return Invalid("Unable to find a group for given key.");
                }

                if (group.Owner.UniqueKey != request.ActorKey)
                {
                    return Invalid("Only the group organizer can record expenses.");
                }

                var membersByKey = group.Members.ToDictionary(member => member.UniqueKey);
                if (!membersByKey.TryGetValue(request.PayerKey, out var payer))
                {
                    return Invalid("The payer must be an active member of the group.");
                }

                var participants = new List<User>();
                foreach (var participantKey in request.ParticipantKeys)
                {
                    if (!membersByKey.TryGetValue(participantKey, out var participant))
                    {
                        return Invalid("Every participant must be an active member of the group.");
                    }

                    participants.Add(participant);
                }

                var expense = new Expense
                {
                    Description = request.Description.Trim(),
                    Currency = "USD",
                    Amount = request.Amount,
                    BelongsTo = group,
                    CreatedBy = group.Owner,
                    Payer = payer,
                    UniqueKey = request.IdempotencyKey,
                    Shares = AllocateShares(request.Amount, participants)
                };

                var (stored, created) = await _expenseRepository.CreateWithSharesAsync(expense, cancellationToken);
                if (!created && !IsSameSubmission(stored, expense))
                {
                    return Invalid("This expense request key was already used for different expense details.");
                }

                return Successful(new CreateExpenseResponse
                {
                    Id = stored.Id,
                    UniqueKey = stored.UniqueKey,
                    Replayed = !created
                });
            }

            return Invalid(validationResult.Errors.Select(x => x.ErrorMessage).ToList());
        }

        private static List<ExpenseShare> AllocateShares(decimal amount, IReadOnlyCollection<User> participants)
        {
            var totalCents = decimal.ToInt64(amount * 100m);
            var participantCount = participants.Count;
            var baseShareCents = totalCents / participantCount;
            var remainderCents = totalCents % participantCount;

            return participants
                .OrderBy(participant => participant.UniqueKey)
                .Select((participant, index) => new ExpenseShare
                {
                    Amount = (baseShareCents + (index < remainderCents ? 1 : 0)) / 100m,
                    Participant = participant
                })
                .ToList();
        }

        private static bool IsSameSubmission(Expense stored, Expense requested)
        {
            return stored.Description == requested.Description
                && stored.Currency == requested.Currency
                && stored.Amount == requested.Amount
                && stored.BelongsTo?.Id == requested.BelongsTo?.Id
                && stored.CreatedBy?.Id == requested.CreatedBy?.Id
                && stored.Payer?.Id == requested.Payer?.Id
                && stored.Shares
                    .OrderBy(share => share.Participant.Id)
                    .Select(share => (share.Participant.Id, share.Amount))
                    .SequenceEqual(requested.Shares
                        .OrderBy(share => share.Participant.Id)
                        .Select(share => (share.Participant.Id, share.Amount)));
        }
    }

    public class CreateExpenseRequest : IRequest<ResponseWrapper<CreateExpenseResponse>>
    {
        public string Description { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public Guid PayerKey { get; set; }
        public List<Guid> ParticipantKeys { get; set; } = [];
        public Guid IdempotencyKey { get; set; }

        public Guid ActorKey { get; set; }
        public Guid GroupKey { get; set; }

        public CreateExpenseRequest()
        {
            Description = string.Empty;
            Currency = string.Empty;
        }
    }

    public class CreateExpenseResponse
    {
        public int Id { get; set; }
        public Guid UniqueKey { get; set; }
        public bool Replayed { get; set; }
    }
}
