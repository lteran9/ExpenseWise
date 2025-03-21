using System;
using Infrastructure.SqlDatabase;
using Microsoft.EntityFrameworkCore;

namespace Tests.Infrastructure
{
   public class MemberOfTests
   {
      [Fact]
      public void Create()
      {
         using (var context = new CoreContext())
         {
            var membership
               = new MemberOf()
               {
                  UserId = 1,
                  GroupId = 1,
                  CreatedAt = DateTime.Now,
                  UpdatedAt = DateTime.Now
               };

            var dbMembership = context.Add(membership);
            var result = context.SaveChanges();

            Assert.True(dbMembership.Entity.Id > 0);
            Assert.True(result > 0);
         }
      }

      [Fact]
      public void Create_Duplicate()
      {
         using (var context = new CoreContext())
         {
            var membership
               = new MemberOf()
               {
                  UserId = 1,
                  GroupId = 1,
                  CreatedAt = DateTime.Now,
                  UpdatedAt = DateTime.Now
               };

            context.Add(membership);
            Assert.Throws<DbUpdateException>(() => context.SaveChanges());
         }
      }
   }
}