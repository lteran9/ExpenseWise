using System;
using AutoMapper;
using Core.Entities;

namespace Infrastructure.SqlDatabase
{
   public class MemberOfProfile : Profile
   {
      public MemberOfProfile()
      {
         // Database to Entity
         CreateMap<MemberOfEntity, MemberOf>()
            .ForMember(d => d.Group, src => src.MapFrom(s => DatabaseMapper.GroupMapper.Map<Group>(s.Group)))
            .ForMember(d => d.User, src => src.MapFrom(s => DatabaseMapper.UserMapper.Map<User>(s.User)));

         // Entity to Database
         CreateMap<MemberOf, MemberOfEntity>()
            .ForMember(d => d.Group, src => src.MapFrom(s => DatabaseMapper.GroupMapper.Map<GroupEntity>(s.Group)))
            .ForMember(d => d.User, src => src.MapFrom(s => DatabaseMapper.UserMapper.Map<UserEntity>(s.User)));
      }
   }
}