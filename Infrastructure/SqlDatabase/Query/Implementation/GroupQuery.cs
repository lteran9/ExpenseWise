using System;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.SqlDatabase
{
   public class GroupQuery : IQuery<GroupEntity>
   {
      public async Task<List<GroupEntity>> Find(GroupEntity entity)
      {
         using (var context = new CoreContext())
         {
            return await context.Groups.Where(x => x.OwnerId == entity.OwnerId).ToListAsync();
         }
      }
   }
}