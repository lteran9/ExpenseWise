using System;
using Application.UseCases;
using Core.Entities;

namespace Infrastructure.SqlDatabase
{
    public class QueryAdapter : IQueryPort<MemberOf>
    {
        private readonly IQuery<MemberOfEntity> _memberOfQuery;

        public QueryAdapter()
        {
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

        private MemberOf MapDatabaseToEntity(MemberOfEntity dbEntity) => DatabaseMapper.Instance.Map<MemberOf>(dbEntity);
        private MemberOfEntity MapEntityToDatabase(MemberOf entity) => DatabaseMapper.Instance.Map<MemberOfEntity>(entity);

        #endregion
    }
}
