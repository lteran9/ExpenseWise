using System;
using AutoMapper;
using Core.Entities;
using OpenAPI;
using UI.Models;

namespace UI.Configuration
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserViewModel>()
                .ForMember(x => x.Id, src => src.MapFrom(s => s.UniqueKey));
            CreateMap<OpenAPI.FindUserResponse, UserViewModel>()
                .ForMember(x => x.Id, src => src.MapFrom(s => s.UniqueKey));
        }
    }
}
