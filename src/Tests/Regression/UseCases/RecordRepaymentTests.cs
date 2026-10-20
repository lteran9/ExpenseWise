using Application.UseCases;
using Application.UseCases.Ports;
using Core.Entities;
using Moq;

namespace Tests.Regression.UseCases
{
    public class RecordRepaymentTests
    {
        private readonly Guid _groupKey = Guid.NewGuid();
        private readonly User _organizer = new() { Id = 1, UniqueKey = Guid.NewGuid(), Name = "Organizer" };
        private readonly User _sender = new() { Id = 2, UniqueKey = Guid.NewGuid(), Name = "Sender" };
        private readonly User _recipient = new() { Id = 3, UniqueKey = Guid.NewGuid(), Name = "Recipient" };
        private readonly Group _group;

        public RecordRepaymentTests()
        {
            _group = new Group
            {
                Id = 9,
                UniqueKey = _groupKey,
                Owner = _organizer,
                Members = [_organizer, _sender, _recipient]
            };
        }

        private RecordRepaymentRequest ValidRequest(Guid? actorKey = null, decimal amount = 5m) => new()
        {
            ActorKey = actorKey ?? _organizer.UniqueKey,
            GroupKey = _groupKey,
            IdempotencyKey = Guid.NewGuid(),
            SenderKey = _sender.UniqueKey,
            RecipientKey = _recipient.UniqueKey,
            Currency = "USD",
            Amount = amount
        };

        private static Mock<IGroupRepository> GroupRepositoryReturning(Group group)
        {
            var repository = new Mock<IGroupRepository>();
            repository.Setup(value => value.FindByUniqueKeyAsync(group.UniqueKey)).ReturnsAsync(group);
            return repository;
        }

        [Fact]
        public async Task RecordRepayment_WhenOrganizerRecordsValidRepayment_ReturnsCreated()
        {
            var request = ValidRequest();
            var repaymentRepository = new Mock<IRepaymentRepository>();
            repaymentRepository.Setup(value => value.RecordAsync(
                    It.IsAny<Repayment>(), It.IsAny<Func<GroupLedger, string?>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Repayment candidate, Func<GroupLedger, string?> _, CancellationToken _) =>
                    new RecordRepaymentOutcome(candidate, true, null));
            var useCase = new RecordRepayment(GroupRepositoryReturning(_group).Object, repaymentRepository.Object);

            var response = await useCase.Handle(request, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.True(response.Result!.Created);
            Assert.Equal(request.IdempotencyKey, response.Result.RepaymentKey);
        }

        [Fact]
        public async Task RecordRepayment_WhenActorIsNotOrganizer_DoesNotRecord()
        {
            var repaymentRepository = new Mock<IRepaymentRepository>();
            var useCase = new RecordRepayment(GroupRepositoryReturning(_group).Object, repaymentRepository.Object);

            var response = await useCase.Handle(ValidRequest(actorKey: _sender.UniqueKey), CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.Contains("Only the group organizer can record repayments.", response.ValidationMessages!);
            repaymentRepository.Verify(value => value.RecordAsync(
                It.IsAny<Repayment>(), It.IsAny<Func<GroupLedger, string?>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RecordRepayment_WhenRecipientIsNotMember_DoesNotRecord()
        {
            var outsider = new User { Id = 4, UniqueKey = Guid.NewGuid() };
            var request = ValidRequest();
            request.RecipientKey = outsider.UniqueKey;
            var repaymentRepository = new Mock<IRepaymentRepository>();
            var useCase = new RecordRepayment(GroupRepositoryReturning(_group).Object, repaymentRepository.Object);

            var response = await useCase.Handle(request, CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.Contains("The recipient must be an active member of the group.", response.ValidationMessages!);
        }

        [Fact]
        public async Task RecordRepayment_WhenLedgerRejectsRepayment_ReturnsReason()
        {
            var repaymentRepository = new Mock<IRepaymentRepository>();
            repaymentRepository.Setup(value => value.RecordAsync(
                    It.IsAny<Repayment>(), It.IsAny<Func<GroupLedger, string?>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RecordRepaymentOutcome(null, false, "The amount exceeds the sender's current debt of 1.00."));
            var useCase = new RecordRepayment(GroupRepositoryReturning(_group).Object, repaymentRepository.Object);

            var response = await useCase.Handle(ValidRequest(amount: 5m), CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.Contains("The amount exceeds the sender's current debt of 1.00.", response.ValidationMessages!);
        }

        [Fact]
        public async Task RecordRepayment_WhenSameKeyReplayedWithSamePayload_ReturnsOriginal()
        {
            var request = ValidRequest(amount: 5m);
            var stored = new Repayment
            {
                UniqueKey = request.IdempotencyKey,
                Currency = "USD",
                Amount = 5m,
                BelongsTo = _group,
                Sender = _sender,
                Recipient = _recipient
            };
            var repaymentRepository = new Mock<IRepaymentRepository>();
            repaymentRepository.Setup(value => value.RecordAsync(
                    It.IsAny<Repayment>(), It.IsAny<Func<GroupLedger, string?>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RecordRepaymentOutcome(stored, false, null));
            var useCase = new RecordRepayment(GroupRepositoryReturning(_group).Object, repaymentRepository.Object);

            var response = await useCase.Handle(request, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.False(response.Result!.Created);
        }

        [Fact]
        public async Task RecordRepayment_WhenKeyReusedWithDifferentPayload_ReturnsInvalid()
        {
            var request = ValidRequest(amount: 7m);
            var stored = new Repayment
            {
                UniqueKey = request.IdempotencyKey,
                Currency = "USD",
                Amount = 5m,
                BelongsTo = _group,
                Sender = _sender,
                Recipient = _recipient
            };
            var repaymentRepository = new Mock<IRepaymentRepository>();
            repaymentRepository.Setup(value => value.RecordAsync(
                    It.IsAny<Repayment>(), It.IsAny<Func<GroupLedger, string?>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RecordRepaymentOutcome(stored, false, null));
            var useCase = new RecordRepayment(GroupRepositoryReturning(_group).Object, repaymentRepository.Object);

            var response = await useCase.Handle(request, CancellationToken.None);

            Assert.False(response.Succeeded);
            Assert.Contains("This repayment key was already used for a different repayment.", response.ValidationMessages!);
        }
    }
}
