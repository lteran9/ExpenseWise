using System;
using Application.UseCases;
using AutoMapper;
using Core.Entities;
using UI.Models;

namespace UI.Configuration
{
    public class GroupProfile : Profile
    {
        public GroupProfile()
        {
            CreateMap<RetrieveGroupResponse, GroupViewModel>()
               .ForMember(x => x.OwnerId, src => src.MapFrom(s => s.OwnerId))
               .ForMember(x => x.Members, src => src.MapFrom(s => s.Members.Select(ModelMapper.UserMapper.Map<UserViewModel>)));
        }
    }
}
