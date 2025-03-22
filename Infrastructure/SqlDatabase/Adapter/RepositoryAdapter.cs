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
      private readonly UserRepository _userRepository;

      public RepositoryAdapter()
      {
         _userRepository = new UserRepository();
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
            var dbUser = await _userRepository.UpdateAsync(MapEntityToDatabase(entity));
            if (dbUser != null)
            {
               return MapDatabaseToEntity(dbUser);
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

      #endregion

      private User MapDatabaseToEntity(UserEntity dbEntity) => DatabaseMapper.UserMapper.Map<User>(dbEntity);
      private UserEntity MapEntityToDatabase(User entity) => DatabaseMapper.UserMapper.Map<UserEntity>(entity);
   }
}