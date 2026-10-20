using AutoMapper;
using Core.Entities;

namespace Infrastructure.SqlDatabase
{
    public class ExpenseShareProfile : Profile
    {
        public ExpenseShareProfile()
        {
            CreateMap<ExpenseShareEntity, ExpenseShare>()
                .ForMember(destination => destination.Participant,
                    source => source.MapFrom(entity => entity.Participant != null
                        ? DatabaseMapper.Instance.Map<User>(entity.Participant)
                        : null))
                .ForMember(destination => destination.Expense, source => source.Ignore());

            CreateMap<ExpenseShare, ExpenseShareEntity>()
                .ForMember(destination => destination.Participant,
                    source => source.MapFrom(entity => entity.Participant != null
                        ? DatabaseMapper.Instance.Map<UserEntity>(entity.Participant)
                        : null))
                .ForMember(destination => destination.Expense, source => source.Ignore())
                .ForMember(destination => destination.ExpenseId,
                    source => source.MapFrom(entity => entity.Expense.Id))
                .ForMember(destination => destination.UserId,
                    source => source.MapFrom(entity => entity.Participant.Id));
        }
    }
}
