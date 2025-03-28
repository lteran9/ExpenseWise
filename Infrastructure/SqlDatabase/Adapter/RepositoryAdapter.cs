using System;
using Application.UseCases.Ports;
using Core.Entities;

namespace Infrastructure.SqlDatabase
{
   /// <summary>
   /// The adapter handles conversion between the Core Entities classes and the Entity Framework models.
   /// </summary>
   public class RepositoryAdapter : IRepository
   {
      private readonly ISqlDatabase<UserEntity> _userRepository;
      private readonly ISqlDatabase<GroupEntity> _groupRepository;

      public RepositoryAdapter()
      {
         _userRepository = new UserRepository();
         _groupRepository = new GroupRepository();
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

      public async Task<User?> GetAsync(User entity)
      {
         if (entity?.Id > 0)
         {
            var dbUser = await _userRepository.GetAsync(MapEntityToDatabase(entity));
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
            var existingRecord = await _userRepository.GetAsync(mappedEntity);
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

      public async Task<Group?> GetAsync(Group entity)
      {
         if (entity?.Id > 0)
         {
            var dbGroup = await _groupRepository.GetAsync(MapEntityToDatabase(entity));
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
   }
}