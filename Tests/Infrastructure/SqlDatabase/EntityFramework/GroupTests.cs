using System;
using Application.UseCases.Ports;
using Infrastructure.SqlDatabase;

namespace Tests.Infrastructure
{
   public class GroupTests
   {
      private readonly ISqlDatabase<GroupEntity> _repository = new GroupRepository();

      [Fact]
      public async Task Create()
      {
         var group =
            new GroupEntity()
            {
               Name = "SampleGroup",
               OwnerId = 1,
               UniqueKey = Guid.NewGuid(),
               CreatedAt = DateTime.Now,
               UpdatedAt = DateTime.Now,
            };

         var dbGroup = await _repository.CreateAsync(group);

         Assert.NotNull(dbGroup);
         Assert.True(dbGroup.Id > 0);
         Assert.True(dbGroup.Active);
      }

      [Fact]
      public async Task Update()
      {
         var group = await _repository.GetAsync(new GroupEntity() { Id = 1 });
      }
   }
}