using System;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.SqlDatabase
{
    public class MemberOfQuery : IQuery<MemberOfEntity>
    {
        private readonly Func<CoreContext> _contextFactory;

        public MemberOfQuery(Func<CoreContext> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        public async Task<List<MemberOfEntity>> FindAsync(MemberOfEntity entity)
        {
            using (var context = _contextFactory())
            {
                return await context.MemberOf.Where(x => x.UserId == entity.UserId).Include(entity => entity.Group).ToListAsync();
            }
        }
    }
}
