using System;
using Microsoft.EntityFrameworkCore;
using Core.Entities;
using Application.UseCases.Ports;

namespace Infrastructure.SqlDatabase
{
    public class GroupRepository : IGroupRepository
    {
        private readonly RepositoryAdapter _adapter = new RepositoryAdapter();

        public async Task<Group?> CreateAsync(Group group)
        {
            var dbEntity = _adapter.MapEntityToDatabase(group);

            using (var context = new CoreContext())
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
            using (var context = new CoreContext())
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
            using (var context = new CoreContext())
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

            using (var context = new CoreContext())
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

            using (var context = new CoreContext())
            {
                var insert = context.Add(dbEntity);
                await context.SaveChangesAsync();
                return _adapter.MapDatabaseToEntity(insert.Entity);
            }
        }

        public async Task<MemberOf?> RemoveMemberAsync(MemberOf member)
        {
            var dbEntity = _adapter.MapEntityToDatabase(member);

            using (var context = new CoreContext())
            {
                var delete = context.Remove(dbEntity);
                await context.SaveChangesAsync();
                return _adapter.MapDatabaseToEntity(delete.Entity);
            }
        }
    }
}
