using System;
using Application.UseCases.Ports;
using Core.Entities;

namespace Infrastructure.SqlDatabase
{
    /// <summary>
    /// The adapter handles conversion between the Core Entities classes and the Entity Framework models.
    /// </summary>
    public class RepositoryAdapter :
       IDatabasePort<User>, IDatabasePort<Group>, IDatabasePort<MemberOf>, IDatabasePort<Expense>, IDatabasePort<Password>, IDatabasePort<Split>
    {
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IRepository<GroupEntity> _groupRepository;
        private readonly IRepository<MemberOfEntity> _memberOfRepository;
        private readonly IRepository<ExpenseEntity> _expenseRepository;
        private readonly IRepository<PasswordEntity> _passwordRepository;
        private readonly IRepository<SplitEntity> _splitRepository;

        public RepositoryAdapter()
        {
            _userRepository = new UserRepository();
            _groupRepository = new GroupRepository();
            _memberOfRepository = new MemberOfRepository();
            _expenseRepository = new ExpenseRepository();
            _passwordRepository = new PasswordRepository();
            _splitRepository = new SplitRepository();
        }

        #region User

        public async Task<User?> CreateAsync(User entity)
        {
            if (entity != null)
            {
                var dbUser = await _userRepository.CreateAsync(MapEntityToDatabase(entity));
                if (dbUser != null)
                {
                    return MapDatabaseToEntity(dbUser);
                }
            }

            return null;
        }

        public async Task<User?> RetrieveAsync(User entity)
        {
            var dbUser = await _userRepository.RetrieveAsync(MapEntityToDatabase(entity));
            if (dbUser != null)
            {
                return MapDatabaseToEntity(dbUser);
            }

            return null;
        }

        public async Task<User?> UpdateAsync(User entity)
        {
            var mappedEntity = MapEntityToDatabase(entity);
            // Get tracked entity
            var existingRecord = await _userRepository.RetrieveAsync(mappedEntity);
            if (existingRecord != null)
            {
                existingRecord.FirstName = mappedEntity.FirstName;
                existingRecord.LastName = mappedEntity.LastName;
                existingRecord.Email = mappedEntity.Email;
                existingRecord.Phone = mappedEntity.Phone;
                existingRecord.UniqueKey = mappedEntity.UniqueKey;
                existingRecord.Active = mappedEntity.Active;

                var updatedRecord = await _userRepository.UpdateAsync(existingRecord);
                if (updatedRecord != null)
                {
                    return MapDatabaseToEntity(updatedRecord);
                }
            }

            return null;
        }

        public async Task<User?> DeleteAsync(User entity)
        {
            var dbUser = await _userRepository.DeleteAsync(MapEntityToDatabase(entity));
            if (dbUser != null)
            {
                return MapDatabaseToEntity(dbUser);
            }

            return null;
        }

        private User MapDatabaseToEntity(UserEntity dbEntity) => DatabaseMapper.Instance.Map<User>(dbEntity);
        private UserEntity MapEntityToDatabase(User entity) => DatabaseMapper.Instance.Map<UserEntity>(entity);

        #endregion

        #region Group

        public async Task<Group?> CreateAsync(Group entity)
        {
            if (entity != null)
            {
                var dbGroup = await _groupRepository.CreateAsync(MapEntityToDatabase(entity));
                if (dbGroup != null)
                {
                    return MapDatabaseToEntity(dbGroup);
                }
            }

            return null;
        }

        public async Task<Group?> RetrieveAsync(Group entity)
        {
            var dbGroup = await _groupRepository.RetrieveAsync(MapEntityToDatabase(entity));
            if (dbGroup != null)
            {
                return MapDatabaseToEntity(dbGroup);
            }

            return null;
        }

        public async Task<Group?> UpdateAsync(Group entity)
        {
            var mappedEntity = MapEntityToDatabase(entity);
            // Get tracked entity
            var existingRecord = await _groupRepository.RetrieveAsync(mappedEntity);
            if (existingRecord != null)
            {
                existingRecord.Name = mappedEntity.Name;
                existingRecord.Active = mappedEntity.Active;
                existingRecord.StartDate = mappedEntity.StartDate;
                existingRecord.EndDate = mappedEntity.EndDate;

                var updatedRecord = await _groupRepository.UpdateAsync(existingRecord);
                if (updatedRecord != null)
                {
                    return MapDatabaseToEntity(updatedRecord);
                }
            }

            return null;
        }

        public async Task<Group?> DeleteAsync(Group entity)
        {
            var dbGroup = await _groupRepository.DeleteAsync(MapEntityToDatabase(entity));
            if (dbGroup != null)
            {
                return MapDatabaseToEntity(dbGroup);
            }

            return null;
        }

        private Group MapDatabaseToEntity(GroupEntity dbEntity) => DatabaseMapper.Instance.Map<Group>(dbEntity);
        private GroupEntity MapEntityToDatabase(Group entity) => DatabaseMapper.Instance.Map<GroupEntity>(entity);

        #endregion

        #region MemberOf

        public async Task<MemberOf?> CreateAsync(MemberOf entity)
        {
            if (entity != null)
            {
                var dbGroup = await _memberOfRepository.CreateAsync(MapEntityToDatabase(entity));
                if (dbGroup != null)
                {
                    return MapDatabaseToEntity(dbGroup);
                }
            }

            return null;
        }

        public async Task<MemberOf?> RetrieveAsync(MemberOf entity)
        {
            var dbGroup = await _memberOfRepository.RetrieveAsync(MapEntityToDatabase(entity));
            if (dbGroup != null)
            {
                return MapDatabaseToEntity(dbGroup);
            }

            return null;
        }

        public async Task<MemberOf?> UpdateAsync(MemberOf entity)
        {
            var dbGroup = await _memberOfRepository.UpdateAsync(MapEntityToDatabase(entity));
            if (dbGroup != null)
            {
                return MapDatabaseToEntity(dbGroup);
            }

            return null;
        }

        public async Task<MemberOf?> DeleteAsync(MemberOf entity)
        {
            var dbGroup = await _memberOfRepository.DeleteAsync(MapEntityToDatabase(entity));
            if (dbGroup != null)
            {
                return MapDatabaseToEntity(dbGroup);
            }

            return null;
        }

        private MemberOf MapDatabaseToEntity(MemberOfEntity dbEntity) => DatabaseMapper.Instance.Map<MemberOf>(dbEntity);
        private MemberOfEntity MapEntityToDatabase(MemberOf entity) => DatabaseMapper.Instance.Map<MemberOfEntity>(entity);

        #endregion

        #region Expense

        public async Task<Expense?> CreateAsync(Expense entity)
        {
            if (entity != null)
            {
                var dbGroup = await _expenseRepository.CreateAsync(MapEntityToDatabase(entity));
                if (dbGroup != null)
                {
                    return MapDatabaseToEntity(dbGroup);
                }
            }

            return null;
        }

        public async Task<Expense?> RetrieveAsync(Expense entity)
        {
            var dbGroup = await _expenseRepository.RetrieveAsync(MapEntityToDatabase(entity));
            if (dbGroup != null)
            {
                return MapDatabaseToEntity(dbGroup);
            }

            return null;
        }

        public async Task<Expense?> UpdateAsync(Expense entity)
        {

            var dbGroup = await _expenseRepository.UpdateAsync(MapEntityToDatabase(entity));
            if (dbGroup != null)
            {
                return MapDatabaseToEntity(dbGroup);
            }


            return null;
        }

        public async Task<Expense?> DeleteAsync(Expense entity)
        {
            var dbGroup = await _expenseRepository.DeleteAsync(MapEntityToDatabase(entity));
            if (dbGroup != null)
            {
                return MapDatabaseToEntity(dbGroup);
            }

            return null;
        }

        private Expense MapDatabaseToEntity(ExpenseEntity dbEntity) => DatabaseMapper.Instance.Map<Expense>(dbEntity);
        private ExpenseEntity MapEntityToDatabase(Expense entity) => DatabaseMapper.Instance.Map<ExpenseEntity>(entity);

        #endregion

        #region Password

        public async Task<Password?> CreateAsync(Password entity)
        {
            if (entity != null)
            {
                var dbPassword = await _passwordRepository.CreateAsync(MapEntityToDatabase(entity));
                if (dbPassword != null)
                {
                    return MapDatabaseToEntity(dbPassword);
                }
            }

            return null;
        }

        public async Task<Password?> RetrieveAsync(Password entity)
        {
            if (entity?.UserId > 0 || entity?.UserId > 0)
            {
                var dbPassword = await _passwordRepository.RetrieveAsync(MapEntityToDatabase(entity));
                if (dbPassword != null)
                {
                    return MapDatabaseToEntity(dbPassword);
                }
            }

            return null;
        }

        public async Task<Password?> UpdateAsync(Password entity)
        {
            var dbPassword = await _passwordRepository.UpdateAsync(MapEntityToDatabase(entity));
            if (dbPassword != null)
            {
                return MapDatabaseToEntity(dbPassword);
            }

            return null;
        }

        public async Task<Password?> DeleteAsync(Password entity)
        {
            var dbPassword = await _passwordRepository.DeleteAsync(MapEntityToDatabase(entity));
            if (dbPassword != null)
            {
                return MapDatabaseToEntity(dbPassword);
            }

            return null;
        }

        private Password MapDatabaseToEntity(PasswordEntity dbEntity) => DatabaseMapper.Instance.Map<Password>(dbEntity);
        private PasswordEntity MapEntityToDatabase(Password entity) => DatabaseMapper.Instance.Map<PasswordEntity>(entity);

        #endregion

        #region Split

        public async Task<Split?> CreateAsync(Split entity)
        {
            var dbSplit = await _splitRepository.CreateAsync(MapEntityToDatabase(entity));
            if (dbSplit != null)
            {
                return MapDatabaseToEntity(dbSplit);
            }

            return null;
        }

        public async Task<Split?> RetrieveAsync(Split entity)
        {
            var dbSplit = await _splitRepository.RetrieveAsync(MapEntityToDatabase(entity));
            if (dbSplit != null)
            {
                return MapDatabaseToEntity(dbSplit);
            }

            return null;
        }

        public async Task<Split?> DeleteAsync(Split entity)
        {
            var dbSplit = await _splitRepository.DeleteAsync(MapEntityToDatabase(entity));
            if (dbSplit != null)
            {
                return MapDatabaseToEntity(dbSplit);
            }

            return null;
        }

        public async Task<Split?> UpdateAsync(Split entity)
        {
            var dbSplit = await _splitRepository.UpdateAsync(MapEntityToDatabase(entity));
            if (dbSplit != null)
            {
                return MapDatabaseToEntity(dbSplit);
            }

            return null;
        }

        private Split MapDatabaseToEntity(SplitEntity dbEntity) => DatabaseMapper.Instance.Map<Split>(dbEntity);
        private SplitEntity MapEntityToDatabase(Split entity) => DatabaseMapper.Instance.Map<SplitEntity>(entity);

        #endregion
    }
}
