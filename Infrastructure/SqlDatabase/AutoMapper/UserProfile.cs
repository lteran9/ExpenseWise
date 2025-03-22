using System;
using AutoMapper;
using Core.Entities;

namespace Infrastructure.SqlDatabase
{
   public class UserProfile : Profile
   {
      public UserProfile()
      {
         // Database to Entity
         CreateMap<UserEntity, User>()
            .ForMember(d => d.Name, src => src.MapFrom(s => s.FirstName + " " + s.LastName));

         // Entity to Database
         CreateMap<User, UserEntity>()
            .ForMember(d => d.FirstName, src => src.MapFrom(s => s.Name.Split(' ', StringSplitOptions.None)[0]))
            .ForMember(d => d.LastName, src => src.MapFrom(s => s.Name.Split(' ', StringSplitOptions.None)[1]));
      }
   }
}