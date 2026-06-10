using System;
using Microsoft.EntityFrameworkCore;
using Core.Entities;
using Application.UseCases.Ports;

namespace Infrastructure.SqlDatabase
{
    public class GroupRepository : IGroupRepository
    {
        private readonly RepositoryAdapter _adapter = new RepositoryAdapter();
        private readonly Func<CoreContext> _contextFactory;

        public GroupRepository()
            : this(() => new CoreContext())
        {
        }

        public GroupRepository(Func<CoreContext> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        /// <summary>
        /// Get all groups a user belongs to.
        /// </summary>
        /// <param name="userKey"></param>
        /// <returns></returns>
        public async Task<List<Group>?> ListAsync(Guid userKey)
        {
            using (var context = _contextFactory())
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.UniqueKey == userKey);
                if (user != null)
                {
                    var groups = await context.Groups
                                    .Include(o => o.Owner)
                                    .Include(m => m.Membership!)
                                    .ThenInclude(x => x.User)
                                    .Where(g => g.Active && g.Membership!.Any(m => m.UserId == user.Id))
                                    .ToListAsync();
                    return groups.Select(x => _adapter.MapDatabaseToEntity(x)).ToList();
                }
            }

            return null;
        }

        public async Task<Group?> CreateAsync(Group group)
        {
            var dbEntity = _adapter.MapEntityToDatabase(group);

            using (var context = _contextFactory())
            {
                // Default values
                if (dbEntity.CreatedAt == DateTime.MinValue) dbEntity.CreatedAt = DateTime.Now;
                if (dbEntity.UpdatedAt == DateTime.MinValue) dbEntity.UpdatedAt = DateTime.Now;
                if (dbEntity.UniqueKey == Guid.Empty) dbEntity.UniqueKey = Guid.NewGuid();

                dbEntity.Owner = null;

                var insert = context.Add(dbEntity);
                await context.SaveChangesAsync();
                return _adapter.MapDatabaseToEntity(insert.Entity);
            }
        }

        public async Task<Group?> FindByIdAsync(int id)
        {
            using (var context = _contextFactory())
            {
                var dbEntity = await context.Groups.FindAsync(id);
                if (dbEntity != null && dbEntity.Active)
                {
                    return _adapter.MapDatabaseToEntity(dbEntity);
                }
            }

            return null;
        }

        public async Task<Group?> FindByUniqueKeyAsync(Guid uniqueKey)
        {
            using (var context = _contextFactory())
            {
                var dbEntity = await context.Groups
                    .Include(g => g.Owner)
                    .FirstOrDefaultAsync(x => x.UniqueKey == uniqueKey && x.Active);

                if (dbEntity != null)
                {
                    return _adapter.MapDatabaseToEntity(dbEntity);
                }
            }

            return null;
        }

        public async Task<Group?> UpdateAsync(Group group)
        {
            var dbEntity = _adapter.MapEntityToDatabase(group);

            // Prevent EF Core from updating/inserting the owner navigation
            // when only the group's scalar values should change.
            dbEntity.Owner = null;

            dbEntity.UpdatedAt = DateTime.Now;

            using (var context = _contextFactory())
            {
                var update = context.Update(dbEntity);
                await context.SaveChangesAsync();
                return _adapter.MapDatabaseToEntity(update.Entity);
            }
        }

        /* Hard deletes involve deleting foreign key relationships as well. */

        public async Task<Group?> DeleteAsync(Group group)
        {
            // Soft delete
            group.Active = false;

            return await UpdateAsync(group);
        }

        public async Task<MemberOf?> AddMemberAsync(MemberOf member)
        {
            var dbEntity = _adapter.MapEntityToDatabase(member);

            // Prevent EF Core from trying to insert/update related User and Group entities
            // when we only want to persist the membership join row.
            dbEntity.User = null;
            dbEntity.Group = null;

            if (dbEntity.CreatedAt == DateTime.MinValue) dbEntity.CreatedAt = DateTime.Now;
            if (dbEntity.UpdatedAt == DateTime.MinValue) dbEntity.UpdatedAt = DateTime.Now;

            using (var context = _contextFactory())
            {
                var insert = context.Add(dbEntity);
                await context.SaveChangesAsync();

                return _adapter.MapDatabaseToEntity(insert.Entity);
            }
        }

        public async Task<MemberOf?> RemoveMemberAsync(MemberOf member)
        {
            var dbEntity = _adapter.MapEntityToDatabase(member);

            using (var context = _contextFactory())
            {
                var stored = dbEntity.Id > 0
                    ? await context.MemberOf.FindAsync(dbEntity.Id)
                    : await context.MemberOf.FirstOrDefaultAsync(x => x.UserId == dbEntity.UserId && x.GroupId == dbEntity.GroupId);

                if (stored == null)
                {
                    return null;
                }

                var delete = context.Remove(stored);
                await context.SaveChangesAsync();
                return _adapter.MapDatabaseToEntity(delete.Entity);
            }
        }
    }
}
