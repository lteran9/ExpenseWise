using System;
using Application.UseCases;
using AutoMapper;
using UI.Models;

namespace UI.Configuration
{
    public class GroupProfile : Profile
    {
        public GroupProfile()
        {
            CreateMap<RetrieveGroupResponse, GroupViewModel>()
                .ForMember(x => x.OwnerId, src => src.MapFrom(s => s.OwnerId))
                .ForMember(x => x.Members, src => src.MapFrom(s => s.Members.Select(ModelMapper.Instance.Map<UserViewModel>)));
        }
    }
}
