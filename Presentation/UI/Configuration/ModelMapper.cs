using System;
using AutoMapper;

namespace UI.Configuration
{
   public static class ModelMapper
   {
      public static IMapper GroupMapper
      {
         get
         {
            return
               new MapperConfiguration(cfg =>
               {
                  cfg.AddProfile(Activator.CreateInstance(typeof(GroupProfile)) as Profile);
               }).CreateMapper();
         }
      }

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