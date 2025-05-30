using System;
using Application.UseCases;
using Core.Entities;

namespace Infrastructure.SqlDatabase
{
   public class QueryAdapter : IQueryPort<Group>, IQueryPort<MemberOf>
   {
      private readonly IQuery<GroupEntity> _groupQuery;
      private readonly IQuery<MemberOfEntity> _memberOfQuery;

      public QueryAdapter()
      {
         _groupQuery = new GroupQuery();
         _memberOfQuery = new MemberOfQuery();
      }

      #region MemberOf

      public async Task<List<MemberOf>> FindAsync(MemberOf entity)
      {
         var dbMemberOf = await _memberOfQuery.FindAsync(MapEntityToDatabase(entity));
         if (dbMemberOf?.Any() == true)
         {
            return dbMemberOf.Select(MapDatabaseToEntity).ToList();
         }

         return new List<MemberOf>();
      }

      private MemberOf MapDatabaseToEntity(MemberOfEntity dbEntity) => DatabaseMapper.MemberOfMapper.Map<MemberOf>(dbEntity);
      private MemberOfEntity MapEntityToDatabase(MemberOf entity) => DatabaseMapper.MemberOfMapper.Map<MemberOfEntity>(entity);

      #endregion

      #region Group 

      public async Task<List<Group>> FindAsync(Group entity)
      {
         var dbGroups = await _groupQuery.FindAsync(MapEntityToDatabase(entity));
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