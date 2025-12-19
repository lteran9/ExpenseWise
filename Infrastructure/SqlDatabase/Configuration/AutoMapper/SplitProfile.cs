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
               .ForMember(d => d.User, src => src.MapFrom(s => DatabaseMapper.UserMapper.Map<User>(s.User)))
               .ForMember(d => d.Group, src => src.MapFrom(s => DatabaseMapper.GroupMapper.Map<Group>(s.Group)))
               .ForMember(d => d.Expense, src => src.MapFrom(s => DatabaseMapper.ExpenseMapper.Map<Expense>(s.Expense)));

            // Entity to Database
            CreateMap<Split, SplitEntity>()
               .ForMember(d => d.User, src => src.MapFrom(s => DatabaseMapper.UserMapper.Map<UserEntity>(s.User)))
               .ForMember(d => d.Group, src => src.MapFrom(s => DatabaseMapper.GroupMapper.Map<GroupEntity>(s.Group)))
               .ForMember(d => d.Expense, src => src.MapFrom(s => DatabaseMapper.ExpenseMapper.Map<ExpenseEntity>(s.Expense)));
        }
    }
}
