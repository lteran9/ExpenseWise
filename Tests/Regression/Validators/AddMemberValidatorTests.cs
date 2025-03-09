using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression
{
   public class AddMemberValidatorTests
   {
      [Theory]
      [AutoMoq]
      public async Task Validate_UserNotNull([Frozen] Mock<ISqlDatabase<MemberOf>> mockRespository)
      {
         var addMember =
            new AddMemberRequest()
            {
               Group = new Group() { Id = 1000 }
            };
         var handler = new AddMember(mockRespository.Object);
         var response = await handler.Handle(addMember, new CancellationToken());

         Assert.False(response.Succeeded);
         Assert.True(response.ValidationMessages?.Any() == true);
      }

      [Theory]
      [AutoMoq]
      public async Task Validate_UserIsValid([Frozen] Mock<ISqlDatabase<MemberOf>> mockRespository)
      {
         var addMember =
            new AddMemberRequest()
            {
               User = new User(),
               Group = new Group() { Id = 1000 }
            };
         var handler = new AddMember(mockRespository.Object);
         var response = await handler.Handle(addMember, new CancellationToken());

         Assert.False(response.Succeeded);
         Assert.True(response.ValidationMessages?.Any() == true);
      }

      [Theory]
      [AutoMoq]
      public async Task Validate_GroupNotNull([Frozen] Mock<ISqlDatabase<MemberOf>> mockRespository)
      {
         var addMember =
            new AddMemberRequest()
            {
               User = new User() { Id = 1000 }
            };
         var handler = new AddMember(mockRespository.Object);
         var response = await handler.Handle(addMember, new CancellationToken());

         Assert.False(response.Succeeded);
         Assert.True(response.ValidationMessages?.Any() == true);
      }

      [Theory]
      [AutoMoq]
      public async Task Validate_GroupIsValid([Frozen] Mock<ISqlDatabase<MemberOf>> mockRespository)
      {
         var addMember =
            new AddMemberRequest()
            {
               User = new User() { Id = 1000 },
               Group = new Group()
            };
         var handler = new AddMember(mockRespository.Object);
         var response = await handler.Handle(addMember, new CancellationToken());

         Assert.False(response.Succeeded);
         Assert.True(response.ValidationMessages?.Any() == true);
      }
   }
}