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
                .ForMember(d => d.BelongsTo, src => src.MapFrom(s => s.Group != null ? DatabaseMapper.Instance.Map<Group>(s.Group) : null))
                .ForMember(d => d.CreatedBy, src => src.MapFrom(s => s.CreatedBy != null ? DatabaseMapper.Instance.Map<User>(s.CreatedBy) : null));

            // Entity to Database
            CreateMap<Expense, ExpenseEntity>()
                .ForMember(d => d.Group, src => src.MapFrom(s => s.BelongsTo != null ? DatabaseMapper.Instance.Map<GroupEntity>(s.BelongsTo) : null))
                .ForMember(d => d.CreatedBy, src => src.MapFrom(s => s.CreatedBy != null ? DatabaseMapper.Instance.Map<UserEntity>(s.CreatedBy) : null))
                .ForMember(d => d.UserId, src => src.MapFrom(s => s.CreatedBy != null ? s.CreatedBy.Id : 0))
                .ForMember(d => d.GroupId, src => src.MapFrom(s => s.BelongsTo != null ? s.BelongsTo.Id : 0));
        }
    }
}
