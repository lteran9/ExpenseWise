using System;
using Application.UseCases.Ports;
using Infrastructure.SqlDatabase;
using Moq;

namespace Tests.Infrastructure.EntityFramework
{
    public class MemberOfTests
    {
        public static IEnumerable<object[]> MemberOfData =>
           new List<object[]>()
           {
            new object[] { new MemberOfEntity() { Id = 1, GroupId = 1, UserId = 1, Active = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now } },
            new object[] { new MemberOfEntity() { Id = 2, GroupId = 1, UserId = 2, Active = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now } },
           };

        [Theory]
        [MemberData(nameof(MemberOfData))]
        public async Task Create(MemberOfEntity record)
        {
            // Arrange
            var mockRepo = new Mock<IDatabasePort<MemberOfEntity>>();
            mockRepo.Setup(x => x.CreateAsync(record)).ReturnsAsync(record);

            // Act
            var dbRecord = await mockRepo.Object.CreateAsync(record);

            // Assert
            mockRepo.Verify(repo => repo.CreateAsync(
               It.Is<MemberOfEntity>(m =>
                  m.Id == dbRecord!.Id &&
                  m.GroupId == dbRecord!.GroupId &&
                  m.UserId == dbRecord!.UserId &&
                  m.Active == dbRecord!.Active &&
                  m.CreatedAt == dbRecord!.CreatedAt &&
                  m.UpdatedAt == dbRecord!.UpdatedAt
               ))
            );
        }

        [Theory]
        [MemberData(nameof(MemberOfData))]
        public async Task Retrieve(MemberOfEntity record)
        {
            // Arrange
            var mockRepo = new Mock<IDatabasePort<MemberOfEntity>>();

            // Act
            var dbRecord = await mockRepo.Object.RetrieveAsync(record);

            // Assert
            mockRepo.Verify(repo => repo.RetrieveAsync(
               It.Is<MemberOfEntity>(m =>
                  m.Id == record.Id &&
                  m.GroupId == record.GroupId &&
                  m.UserId == record.UserId &&
                  m.Active == record.Active &&
                  m.CreatedAt == record.CreatedAt &&
                  m.UpdatedAt == record.UpdatedAt
               ))
            );
        }

        [Theory]
        [MemberData(nameof(MemberOfData))]
        public async Task Update(MemberOfEntity record)
        {
            // Arrange
            var mockRepo = new Mock<IDatabasePort<MemberOfEntity>>();

            // Act
            var dbRecord = await mockRepo.Object.UpdateAsync(record);

            // Assert
            mockRepo.Verify(repo => repo.UpdateAsync(
               It.Is<MemberOfEntity>(m =>
                  m.Id == record.Id &&
                  m.GroupId == record.GroupId &&
                  m.UserId == record.UserId &&
                  m.Active == record.Active &&
                  m.CreatedAt == record.CreatedAt &&
                  m.UpdatedAt == record.UpdatedAt
               ))
            );
        }

        [Theory]
        [MemberData(nameof(MemberOfData))]
        public async Task Delete(MemberOfEntity record)
        {
            // Arrange
            var mockRepo = new Mock<IDatabasePort<MemberOfEntity>>();

            // Act
            var dbRecord = await mockRepo.Object.DeleteAsync(record);

            // Assert
            mockRepo.Verify(repo => repo.DeleteAsync(
               It.Is<MemberOfEntity>(m =>
                  m.Id == record.Id &&
                  m.GroupId == record.GroupId &&
                  m.UserId == record.UserId &&
                  m.Active == record.Active &&
                  m.CreatedAt == record.CreatedAt &&
                  m.UpdatedAt == record.UpdatedAt
               ))
            );
        }
    }
}
