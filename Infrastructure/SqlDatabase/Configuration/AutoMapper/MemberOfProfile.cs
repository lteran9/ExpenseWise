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
                .ForMember(d => d.Group, src => src.MapFrom(s => DatabaseMapper.Instance.Map<Group>(s.Group)))
                .ForMember(d => d.User, src => src.MapFrom(s => DatabaseMapper.Instance.Map<User>(s.User)));

            // Entity to Database
            CreateMap<MemberOf, MemberOfEntity>()
                .ForMember(d => d.GroupId, src => src.MapFrom(s => s.Group.Id))
                .ForMember(d => d.Group, src => src.MapFrom(s => DatabaseMapper.Instance.Map<GroupEntity>(s.Group)))
                .ForMember(d => d.UserId, src => src.MapFrom(s => s.User.Id))
                .ForMember(d => d.User, src => src.MapFrom(s => DatabaseMapper.Instance.Map<UserEntity>(s.User)));
        }
    }
}
