using System;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.SqlDatabase
{
    public class GroupQuery : IQuery<GroupEntity>
    {
        public async Task<List<GroupEntity>> FindAsync(GroupEntity entity)
        {
            using (var context = new CoreContext())
            {
                return await context.Groups
                   .Include(group => group.Membership!)
                   .ThenInclude(member => member.User)
                   .Where(x => x.OwnerId == entity.OwnerId)
                   .ToListAsync();
            }
        }
    }
}
