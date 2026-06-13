using System;
using AutoMapper;
using Core.Entities;

namespace Infrastructure.SqlDatabase
{
    public class ExpenseProfile : Profile
    {
        public ExpenseProfile()
        {
            // Database to Entity
            CreateMap<ExpenseEntity, Expense>()
                .ForMember(d => d.BelongsTo, src => src.MapFrom(s => DatabaseMapper.Instance.Map<Group>(s.Group)));

            // Entity to Database
            CreateMap<Expense, ExpenseEntity>()
                .ForMember(d => d.Group, src => src.MapFrom(s => DatabaseMapper.Instance.Map<GroupEntity>(s.BelongsTo)))
                .ForMember(d => d.UserId, src => src.MapFrom(s => s.CreatedBy.Id))
                .ForMember(d => d.GroupId, src => src.MapFrom(s => s.BelongsTo.Id));
        }
    }
}
