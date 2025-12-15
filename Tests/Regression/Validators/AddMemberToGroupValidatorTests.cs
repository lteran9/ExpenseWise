using System;
using Application.UseCases;
using Application.UseCases.Ports;
using AutoFixture.Xunit2;
using Core.Entities;
using Moq;

namespace Tests.Regression.Validators
{
    public class AddMemberToGroupValidatorTests
    {
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
#pragma warning disable IDE0060 // Remove unused parameter
        [Theory]
        [AutoMoq]
        public async Task Validate_UserPhoneNotNull(
           [Frozen] Mock<IDatabasePort<MemberOf>> mockRepository,
           AddMemberToGroup useCase)
        {
            // Arrange
            var addMember =
               new AddMemberToGroupRequest()
               {
                   GroupUniqueKey = Guid.NewGuid()
               };

            // Act
            var response = await useCase.Handle(addMember, new CancellationToken());

            // Assert
            Assert.False(response.Succeeded);
            Assert.True(response.ValidationMessages?.Any() == true);
        }

        [Theory]
        [AutoMoq]
        public async Task Validate_GroupNotNull(
           [Frozen] Mock<IDatabasePort<MemberOf>> mockRepository,
           AddMemberToGroup useCase)
        {
            // Arrange
            var addMember =
               new AddMemberToGroupRequest()
               {
                   Phone = "+1 (602) 333-4567"
               };

            // Act
            var response = await useCase.Handle(addMember, new CancellationToken());

            // Assert
            Assert.False(response.Succeeded);
            Assert.True(response.ValidationMessages?.Any() == true);
        }
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
    }
}