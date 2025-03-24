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
            .ForMember(d => d.Name, src => src.MapFrom(s => !string.IsNullOrEmpty(s.LastName) ? s.FirstName + " " + s.LastName : s.FirstName));

         // Entity to Database
         CreateMap<User, UserEntity>()
            .ForMember(d => d.FirstName, src => src.MapFrom(s => s.Name.IndexOf(' ') > 0 ? s.Name.Substring(0, s.Name.IndexOf(' ')) : s.Name))
            .ForMember(d => d.LastName, src => src.MapFrom(s => s.Name.IndexOf(' ') > 0 ? s.Name.Substring(s.Name.IndexOf(' ') + 1) : string.Empty));
      }
   }
}