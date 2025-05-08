using System;
using Application.UseCases;
using Core.Entities;

namespace Infrastructure.SqlDatabase
{
   public class QueryAdapter : IQueryPort<Group>
   {
      private readonly IQuery<GroupEntity> _query;

      public QueryAdapter()
      {
         _query = new GroupQuery();
      }

      #region Group 

      public async Task<List<Group>> FindAsync(Group entity)
      {
         var dbGroups = await _query.Find(MapEntityToDatabase(entity));
         if (dbGroups?.Any() == true)
         {
            var groups = dbGroups.Select(MapDatabaseToEntity).ToList();

            // Reapply owner without polling database
            foreach (var group in groups)
            {
               group.Owner = entity.Owner;
            }

            return groups;
         }

         return new List<Group>();
      }

      private Group MapDatabaseToEntity(GroupEntity dbEntity) => DatabaseMapper.GroupMapper.Map<Group>(dbEntity);
      private GroupEntity MapEntityToDatabase(Group entity) => DatabaseMapper.GroupMapper.Map<GroupEntity>(entity);

      #endregion
   }
}