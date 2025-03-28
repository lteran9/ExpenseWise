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

         };

      [Theory]
      [MemberData(nameof(MemberOfData))]
      public async Task Create(MemberOfEntity record)
      {
         // Arrange
         var mockRepo = new Mock<ISqlDatabase<MemberOfEntity>>();

         // Act 
         var dbRecord = await mockRepo.Object.CreateAsync(record);

         // Assert
         mockRepo.Verify(repo => repo.CreateAsync(
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
      public async Task Retrieve(MemberOfEntity record)
      {
         // Arrange
         var mockRepo = new Mock<ISqlDatabase<MemberOfEntity>>();

         // Act 
         var dbRecord = await mockRepo.Object.GetAsync(record);

         // Assert
         mockRepo.Verify(repo => repo.GetAsync(
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
         var mockRepo = new Mock<ISqlDatabase<MemberOfEntity>>();

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
         var mockRepo = new Mock<ISqlDatabase<MemberOfEntity>>();

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