using System;
using Infrastructure.SqlDatabase;

namespace Tests.Infrastructure
{
   public class UserTests
   {
      [Fact]
      public void Create()
      {
         using (var context = new CoreContext())
         {
            var user =
               new User()
               {
                  FirstName = "Test",
                  LastName = "Tester",
                  Email = "test@example.com",
                  Phone = "6023334578",
                  Password = "emptypassword",
                  CreatedAt = DateTime.Now,
                  UpdatedAt = DateTime.Now
               };

            var dbUser = context.Add(user);
            var result = context.SaveChanges();

            Assert.True(dbUser.Entity.Id > 0);
            Assert.True(result > 0);
         }
      }

      [Fact]
      public void Update()
      {
         using (var context = new CoreContext())
         {
            var dbUser = context.Users.Find(1);

            Assert.NotNull(dbUser);

            // Change name
            dbUser.FirstName = "New Test";
            dbUser.UpdatedAt = DateTime.Now;
            var result = context.SaveChanges();

            Assert.True(result > 0);
         }
      }

      [Fact]
      public void HardDelete()
      {
         using (var context = new CoreContext())
         {
            var dbUser = context.Users.Find(1);

            Assert.NotNull(dbUser);

            context.Remove(dbUser!);
            var result = context.SaveChanges();

            Assert.True(result > 0);
         }
      }

      [Fact]
      public void SoftDelete()
      {
         using (var context = new CoreContext())
         {
            var dbUser = context.Users.Find(1);

            Assert.NotNull(dbUser);

            // Change name
            dbUser.Active = false;
            dbUser.UpdatedAt = DateTime.Now;
            var result = context.SaveChanges();

            Assert.True(result > 0);
         }
      }
   }
}