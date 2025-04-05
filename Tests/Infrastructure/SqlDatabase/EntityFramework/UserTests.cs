using System;
using Application.UseCases.Ports;
using Infrastructure.SqlDatabase;
using Moq;

namespace Tests.Infrastructure.EntityFramework
{
   public class UserTests
   {
      public static IEnumerable<object[]> UserEntityData =>
         new List<object[]>()
         {
            new object[] { new UserEntity() { Id = 1, FirstName = "Test", LastName = "Tester", Email = "tester@test.com", UniqueKey = Guid.NewGuid(), CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now } },
            new object[] { new UserEntity() { Id = 2, FirstName = "Jane", LastName = "Doe", Email = "j.doe@test.com", UniqueKey = Guid.NewGuid(), CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now } }
         };

      [Theory]
      [MemberData(nameof(UserEntityData))]
      public async Task Create(UserEntity user)
      {
         // Arrange
         var mockRepo = new Mock<IDatabasePort<UserEntity>>();

         // Act 
         var dbUser = await mockRepo.Object.CreateAsync(user);

         // Assert
         mockRepo.Verify(repo => repo.CreateAsync(
            It.Is<UserEntity>(u =>
               u.Id == user.Id &&
               u.FirstName == user.FirstName &&
               u.LastName == user.LastName &&
               u.Email == user.Email &&
               u.UniqueKey == user.UniqueKey &&
               u.CreatedAt == user.CreatedAt &&
               u.UpdatedAt == user.UpdatedAt
            ))
         );
      }

      [Theory]
      [MemberData(nameof(UserEntityData))]
      public async Task Retrieve(UserEntity user)
      {
         // Arrange
         var mockRepo = new Mock<IDatabasePort<UserEntity>>();

         // Act 
         var dbUser = await mockRepo.Object.RetrieveAsync(user);

         // Assert
         mockRepo.Verify(repo => repo.RetrieveAsync(
            It.Is<UserEntity>(u =>
               u.Id == user.Id &&
               u.FirstName == user.FirstName &&
               u.LastName == user.LastName &&
               u.Email == user.Email &&
               u.UniqueKey == user.UniqueKey &&
               u.CreatedAt == user.CreatedAt &&
               u.UpdatedAt == user.UpdatedAt
            ))
         );
      }

      [Theory]
      [MemberData(nameof(UserEntityData))]
      public async Task Update(UserEntity user)
      {
         // Arrange
         var mockRepo = new Mock<IDatabasePort<UserEntity>>();

         // Act 
         var dbUser = await mockRepo.Object.UpdateAsync(user);

         // Assert
         mockRepo.Verify(repo => repo.UpdateAsync(
            It.Is<UserEntity>(u =>
               u.Id == user.Id &&
               u.FirstName == user.FirstName &&
               u.LastName == user.LastName &&
               u.Email == user.Email &&
               u.UniqueKey == user.UniqueKey &&
               u.CreatedAt == user.CreatedAt &&
               u.UpdatedAt == user.UpdatedAt
            ))
         );
      }

      [Theory]
      [MemberData(nameof(UserEntityData))]
      public async Task Delete(UserEntity user)
      {
         // Arrange
         var mockRepo = new Mock<IDatabasePort<UserEntity>>();

         // Act 
         var dbUser = await mockRepo.Object.DeleteAsync(user);

         // Assert
         mockRepo.Verify(repo => repo.DeleteAsync(
            It.Is<UserEntity>(u =>
               u.Id == user.Id &&
               u.FirstName == user.FirstName &&
               u.LastName == user.LastName &&
               u.Email == user.Email &&
               u.UniqueKey == user.UniqueKey &&
               u.CreatedAt == user.CreatedAt &&
               u.UpdatedAt == user.UpdatedAt
            ))
         );
      }
   }
}