using System;
using AutoMapper;
using Core.Entities;
using UI.Models;

namespace UI.Configuration
{
   public class GroupProfile : Profile
   {
      public GroupProfile()
      {
         CreateMap<Group, GroupViewModel>()
            .ForMember(x => x.OwnerId, src => src.MapFrom(s => s.Owner.UniqueKey))
            .ForMember(x => x.Members, src => src.MapFrom(s => s.Members.Select(ModelMapper.UserMapper.Map<UserViewModel>).ToList()));
      }
   }
}