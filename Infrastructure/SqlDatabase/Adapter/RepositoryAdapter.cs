using System;
using Application.UseCases.Ports;
using Core.Entities;

namespace Infrastructure.SqlDatabase
{
   /// <summary>
   /// The adapter handles conversion between the Core Entities classes and the Entity Framework models.
   /// </summary>
   public class RepositoryAdapter :
      IDatabasePort<User>, IDatabasePort<Group>, IDatabasePort<MemberOf>, IDatabasePort<Expense>
   {
      private readonly IRepository<UserEntity> _userRepository;
      private readonly IRepository<GroupEntity> _groupRepository;
      private readonly IRepository<MemberOfEntity> _memberOfRepository;
      private readonly IRepository<ExpenseEntity> _expenseRepository;

      public RepositoryAdapter()
      {
         _userRepository = new UserRepository();
         _groupRepository = new GroupRepository();
         _memberOfRepository = new MemberOfRepository();
         _expenseRepository = new ExpenseRepository();
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
         if (entity?.Id > 0)
         {
            var dbUser = await _userRepository.RetrieveAsync(MapEntityToDatabase(entity));
            if (dbUser != null)
            {
               return MapDatabaseToEntity(dbUser);
            }
         }

         return null;
      }

      public async Task<User?> UpdateAsync(User entity)
      {
         if (entity?.Id > 0)
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
         }

         return null;
      }

      public async Task<User?> DeleteAsync(User entity)
      {
         if (entity?.Id > 0)
         {
            var dbUser = await _userRepository.DeleteAsync(MapEntityToDatabase(entity));
            if (dbUser != null)
            {
               return MapDatabaseToEntity(dbUser);
            }
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
         if (entity?.Id > 0)
         {
            var dbGroup = await _groupRepository.RetrieveAsync(MapEntityToDatabase(entity));
            if (dbGroup != null)
            {
               return MapDatabaseToEntity(dbGroup);
            }
         }

         return null;
      }

      public async Task<Group?> UpdateAsync(Group entity)
      {
         if (entity?.Id > 0)
         {
            var dbGroup = await _groupRepository.UpdateAsync(MapEntityToDatabase(entity));
            if (dbGroup != null)
            {
               return MapDatabaseToEntity(dbGroup);
            }
         }

         return null;
      }

      public async Task<Group?> DeleteAsync(Group entity)
      {
         if (entity?.Id > 0)
         {
            var dbGroup = await _groupRepository.DeleteAsync(MapEntityToDatabase(entity));
            if (dbGroup != null)
            {
               return MapDatabaseToEntity(dbGroup);
            }
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
         if (entity?.Id > 0)
         {
            var dbGroup = await _memberOfRepository.RetrieveAsync(MapEntityToDatabase(entity));
            if (dbGroup != null)
            {
               return MapDatabaseToEntity(dbGroup);
            }
         }

         return null;
      }

      public async Task<MemberOf?> UpdateAsync(MemberOf entity)
      {
         if (entity?.Id > 0)
         {
            var dbGroup = await _memberOfRepository.UpdateAsync(MapEntityToDatabase(entity));
            if (dbGroup != null)
            {
               return MapDatabaseToEntity(dbGroup);
            }
         }

         return null;
      }

      public async Task<MemberOf?> DeleteAsync(MemberOf entity)
      {
         if (entity?.Id > 0)
         {
            var dbGroup = await _memberOfRepository.DeleteAsync(MapEntityToDatabase(entity));
            if (dbGroup != null)
            {
               return MapDatabaseToEntity(dbGroup);
            }
         }

         return null;
      }

      private MemberOf MapDatabaseToEntity(MemberOfEntity dbEntity) => DatabaseMapper.GroupMapper.Map<MemberOf>(dbEntity);
      private MemberOfEntity MapEntityToDatabase(MemberOf entity) => DatabaseMapper.GroupMapper.Map<MemberOfEntity>(entity);

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
         if (entity?.Id > 0)
         {
            var dbGroup = await _expenseRepository.RetrieveAsync(MapEntityToDatabase(entity));
            if (dbGroup != null)
            {
               return MapDatabaseToEntity(dbGroup);
            }
         }

         return null;
      }

      public async Task<Expense?> UpdateAsync(Expense entity)
      {
         if (entity?.Id > 0)
         {
            var dbGroup = await _expenseRepository.UpdateAsync(MapEntityToDatabase(entity));
            if (dbGroup != null)
            {
               return MapDatabaseToEntity(dbGroup);
            }
         }

         return null;
      }

      public async Task<Expense?> DeleteAsync(Expense entity)
      {
         if (entity?.Id > 0)
         {
            var dbGroup = await _expenseRepository.DeleteAsync(MapEntityToDatabase(entity));
            if (dbGroup != null)
            {
               return MapDatabaseToEntity(dbGroup);
            }
         }

         return null;
      }

      private Expense MapDatabaseToEntity(ExpenseEntity dbEntity) => DatabaseMapper.ExpenseMapper.Map<Expense>(dbEntity);
      private ExpenseEntity MapEntityToDatabase(Expense entity) => DatabaseMapper.ExpenseMapper.Map<ExpenseEntity>(entity);

      #endregion
   }
}