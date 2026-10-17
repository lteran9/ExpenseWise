using System;
using AutoMapper;
using OpenAPI;
using UI.Models;

namespace UI.Configuration
{
    public class GroupProfile : Profile
    {
        public GroupProfile()
        {
            CreateMap<RetrieveGroupResponse, GroupViewModel>();
        }
    }
}
