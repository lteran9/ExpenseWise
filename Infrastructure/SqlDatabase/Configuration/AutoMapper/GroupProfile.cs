using System;
using AutoMapper;
using Core.Entities;

namespace Infrastructure.SqlDatabase
{
   public class GroupProfile : Profile
   {
      public GroupProfile()
      {
         // Database to Entity
         CreateMap<GroupEntity, Group>()
            .ForMember(d => d.Owner, src => src.MapFrom(s => DatabaseMapper.UserMapper.Map<User>(s.Owner)));
         // Entity to Database
         CreateMap<Group, GroupEntity>()
            .ForMember(d => d.OwnerId, src => src.MapFrom(s => s.Owner.Id))
            .ForMember(d => d.Owner, src => src.MapFrom(s => DatabaseMapper.UserMapper.Map<UserEntity>(s.Owner)));
      }
   }
}