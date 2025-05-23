using System;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.SqlDatabase
{
   public class MemberOfQuery : IQuery<MemberOfEntity>
   {
      public async Task<List<MemberOfEntity>> Find(MemberOfEntity entity)
      {
         using (var context = new CoreContext())
         {
            return await context.MemberOf.Where(x => x.UserId == entity.UserId).Include(entity => entity.Group).ToListAsync();
         }
      }
   }
}