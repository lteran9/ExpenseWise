using System;
using Application.UseCases.Ports;
using Core.Entities;

namespace Infrastructure.SqlDatabase
{
   /// <summary>
   /// The adapter handles conversion between the Core Entities classes and the Entity Framework models.
   /// </summary>
   public class RepositoryAdapter :
      IDatabasePort<User>, IDatabasePort<Group>, IDatabasePort<MemberOf>, IDatabasePort<Expense>, IDatabasePort<Password>
   {
      private readonly IRepository<UserEntity> _userRepository;
      private readonly IRepository<GroupEntity> _groupRepository;
      private readonly IRepository<MemberOfEntity> _memberOfRepository;
      private readonly IRepository<ExpenseEntity> _expenseRepository;
      private readonly IRepository<PasswordEntity> _passwordRepository;

      public RepositoryAdapter()
      {
         _userRepository = new UserRepository();
         _groupRepository = new GroupRepository();
         _memberOfRepository = new MemberOfRepository();
         _expenseRepository = new ExpenseRepository();
         _passwordRepository = new PasswordRepository();
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
         var existingRecord = await _userRepository.RetrieveAsync(mappedEntity);
         if (existingRecord != null)
         {
            existingRecord.FirstName = mappedEntity.FirstName;
            existingRecord.LastName = mappedEntity.LastName;
            existingRecord.Email = mappedEntity.Email;
            existingRecord.Phone = mappedEntity.Phone;
            existingRecord.UniqueKey = mappedEntity.UniqueKey;
            existingRecord.Active = mappedEntity.Active;
            existingRecord.UpdatedAt = DateTime.Now;

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

      private User MapDatabaseToEntity(UserEntity dbEntity) => DatabaseMapper.UserMapper.Map<User>(dbEntity);
      private UserEntity MapEntityToDatabase(User entity) => DatabaseMapper.UserMapper.Map<UserEntity>(entity);

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
         var dbGroup = await _groupRepository.UpdateAsync(MapEntityToDatabase(entity));
         if (dbGroup != null)
         {
            return MapDatabaseToEntity(dbGroup);
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

      private Group MapDatabaseToEntity(GroupEntity dbEntity) => DatabaseMapper.GroupMapper.Map<Group>(dbEntity);
      private GroupEntity MapEntityToDatabase(Group entity) => DatabaseMapper.GroupMapper.Map<GroupEntity>(entity);

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

      private MemberOf MapDatabaseToEntity(MemberOfEntity dbEntity) => DatabaseMapper.MemberOfMapper.Map<MemberOf>(dbEntity);
      private MemberOfEntity MapEntityToDatabase(MemberOf entity) => DatabaseMapper.MemberOfMapper.Map<MemberOfEntity>(entity);

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

      private Expense MapDatabaseToEntity(ExpenseEntity dbEntity) => DatabaseMapper.ExpenseMapper.Map<Expense>(dbEntity);
      private ExpenseEntity MapEntityToDatabase(Expense entity) => DatabaseMapper.ExpenseMapper.Map<ExpenseEntity>(entity);

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

      private Password MapDatabaseToEntity(PasswordEntity dbEntity) => DatabaseMapper.PasswordMapper.Map<Password>(dbEntity);
      private PasswordEntity MapEntityToDatabase(Password entity) => DatabaseMapper.PasswordMapper.Map<PasswordEntity>(entity);


      #endregion
   }
}