using System;
using Application.UseCases.Ports;
using Core.Entities;

namespace Infrastructure.SqlDatabase
{
    /// <summary>
    /// The adapter handles conversion between the Core Entities classes and the Entity Framework models.
    /// </summary>
    internal class RepositoryAdapter
    {
        #region User

        public User MapDatabaseToEntity(UserEntity dbEntity) => DatabaseMapper.Instance.Map<User>(dbEntity);
        public UserEntity MapEntityToDatabase(User entity) => DatabaseMapper.Instance.Map<UserEntity>(entity);

        #endregion

        #region Group

        public Group MapDatabaseToEntity(GroupEntity dbEntity) => DatabaseMapper.Instance.Map<Group>(dbEntity);
        public GroupEntity MapEntityToDatabase(Group entity) => DatabaseMapper.Instance.Map<GroupEntity>(entity);

        #endregion

        #region MemberOf

        public MemberOf MapDatabaseToEntity(MemberOfEntity dbEntity) => DatabaseMapper.Instance.Map<MemberOf>(dbEntity);
        public MemberOfEntity MapEntityToDatabase(MemberOf entity) => DatabaseMapper.Instance.Map<MemberOfEntity>(entity);

        #endregion

        #region Expense

        public Expense MapDatabaseToEntity(ExpenseEntity dbEntity) => DatabaseMapper.Instance.Map<Expense>(dbEntity);
        public ExpenseEntity MapEntityToDatabase(Expense entity) => DatabaseMapper.Instance.Map<ExpenseEntity>(entity);

        #endregion

        #region Password

        public Password MapDatabaseToEntity(PasswordEntity dbEntity) => DatabaseMapper.Instance.Map<Password>(dbEntity);
        public PasswordEntity MapEntityToDatabase(Password entity) => DatabaseMapper.Instance.Map<PasswordEntity>(entity);

        #endregion

        #region Split

        public Split MapDatabaseToEntity(SplitEntity dbEntity) => DatabaseMapper.Instance.Map<Split>(dbEntity);
        public SplitEntity MapEntityToDatabase(Split entity) => DatabaseMapper.Instance.Map<SplitEntity>(entity);

        #endregion
    }
}
