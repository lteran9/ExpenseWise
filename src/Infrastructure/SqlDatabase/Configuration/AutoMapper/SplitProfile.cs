using System;
using AutoMapper;
using Core.Entities;

namespace Infrastructure.SqlDatabase
{
    public class SplitProfile : Profile
    {
        public SplitProfile()
        {
            // Database to Entity
            CreateMap<SplitEntity, Split>()
                .ForMember(d => d.User, src => src.MapFrom(s => DatabaseMapper.Instance.Map<User>(s.User)))
                .ForMember(d => d.Group, src => src.MapFrom(s => DatabaseMapper.Instance.Map<Group>(s.Group)))
                .ForMember(d => d.Expense, src => src.MapFrom(s => DatabaseMapper.Instance.Map<Expense>(s.Expense)));

            // Entity to Database
            CreateMap<Split, SplitEntity>()
                .ForMember(d => d.User, src => src.MapFrom(s => DatabaseMapper.Instance.Map<UserEntity>(s.User)))
                .ForMember(d => d.Group, src => src.MapFrom(s => DatabaseMapper.Instance.Map<GroupEntity>(s.Group)))
                .ForMember(d => d.Expense, src => src.MapFrom(s => DatabaseMapper.Instance.Map<ExpenseEntity>(s.Expense)));
        }
    }
}
