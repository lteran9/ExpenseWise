using System;
using Infrastructure.SqlDatabase;

namespace Tests.Infrastructure
{
   public class GroupTests
   {
      [Fact]
      public void Create()
      {
         using (var context = new CoreContext())
         {
            var group =
               new Group()
               {
                  Name = "SampleGroup",
                  OwnerId = 1,
                  UniqueKey = Guid.NewGuid(),
                  CreatedAt = DateTime.Now,
                  UpdatedAt = DateTime.Now,
               };

            var dbGroup = context.Add(group);
            var result = context.SaveChanges();

            Assert.True(dbGroup.Entity.Id > 0);
            Assert.True(result > 0);
         }
      }
   }
}