using System;
using AutoMapper;

namespace Infrastructure.SqlDatabase
{
   public static class DatabaseMapper
   {
      public static IMapper UserMapper
      {
         get
         {
            return
               new MapperConfiguration(cfg =>
               {
                  cfg.AddProfile(Activator.CreateInstance(typeof(UserProfile)) as Profile);
               }).CreateMapper();
         }
      }
   }
}