using System;
using Application.UseCases.Ports;
using Infrastructure.SqlDatabase;
using Moq;

namespace Tests.Infrastructure.EntityFramework
{
    public class GroupTests
    {
        public static IEnumerable<object[]> GroupEntityData =>
            new List<object[]>()
            {
                new object[] { new GroupEntity() { Id = 1, Name = "SampleGroup", OwnerId = 1, UniqueKey = Guid.NewGuid(), CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now } },
                new object[] { new GroupEntity() { Id = 2, Name = "RandomGroup", OwnerId = 2, UniqueKey = Guid.NewGuid(), CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now } }
            };

        [Theory]
        [MemberData(nameof(GroupEntityData))]
        public async Task Create(GroupEntity group)
        {
            // Arrange
            var mockRepo = new Mock<IDatabasePort<GroupEntity>>();

            // Act
            var dbGroup = await mockRepo.Object.CreateAsync(group);

            // Assert
            mockRepo.Verify(repo => repo.CreateAsync(
                It.Is<GroupEntity>(g =>
                    g.Id == group.Id &&
                    g.Name == group.Name &&
                    g.OwnerId == group.OwnerId &&
                    g.UniqueKey == group.UniqueKey &&
                    g.CreatedAt == group.CreatedAt &&
                    g.UpdatedAt == group.UpdatedAt
                ))
            );
        }

        [Theory]
        [MemberData(nameof(GroupEntityData))]
        public async Task Retrieve(GroupEntity group)
        {
            // Arrange
            var mockRepo = new Mock<IDatabasePort<GroupEntity>>();

            // Act
            var dbGroup = await mockRepo.Object.RetrieveAsync(group);

            // Assert
            mockRepo.Verify(repo => repo.RetrieveAsync(
                It.Is<GroupEntity>(g =>
                    g.Id == group.Id &&
                    g.Name == group.Name &&
                    g.OwnerId == group.OwnerId &&
                    g.UniqueKey == group.UniqueKey &&
                    g.CreatedAt == group.CreatedAt &&
                    g.UpdatedAt == group.UpdatedAt
                ))
           );
        }

        [Theory]
        [MemberData(nameof(GroupEntityData))]
        public async Task Update(GroupEntity group)
        {
            // Arrange
            var mockRepo = new Mock<IDatabasePort<GroupEntity>>();

            // Act
            var dbGroup = await mockRepo.Object.UpdateAsync(group);

            // Assert
            mockRepo.Verify(repo => repo.UpdateAsync(
                It.Is<GroupEntity>(g =>
                    g.Id == group.Id &&
                    g.Name == group.Name &&
                    g.OwnerId == group.OwnerId &&
                    g.UniqueKey == group.UniqueKey &&
                    g.CreatedAt == group.CreatedAt &&
                    g.UpdatedAt == group.UpdatedAt
                ))
            );
        }

        [Theory]
        [MemberData(nameof(GroupEntityData))]
        public async Task Delete(GroupEntity group)
        {
            // Arrange
            var mockRepo = new Mock<IDatabasePort<GroupEntity>>();

            // Act
            var dbGroup = await mockRepo.Object.DeleteAsync(group);

            // Assert
            mockRepo.Verify(repo => repo.DeleteAsync(
                It.Is<GroupEntity>(g =>
                    g.Id == group.Id &&
                    g.Name == group.Name &&
                    g.OwnerId == group.OwnerId &&
                    g.UniqueKey == group.UniqueKey &&
                    g.CreatedAt == group.CreatedAt &&
                    g.UpdatedAt == group.UpdatedAt
                ))
            );
        }
    }
}
