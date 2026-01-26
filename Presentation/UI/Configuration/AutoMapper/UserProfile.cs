using System;
using Application.UseCases;
using AutoMapper;
using Core.Entities;
using UI.Models;

namespace UI.Configuration
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserViewModel>()
                .ForMember(x => x.Id, src => src.MapFrom(s => s.UniqueKey));
            CreateMap<FindUserResponse, UserViewModel>()
                .ForMember(x => x.Id, src => src.MapFrom(s => s.UniqueKey));
        }
    }
}
