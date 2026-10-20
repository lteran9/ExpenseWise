using AutoMapper;
using Core.Entities;

namespace Infrastructure.SqlDatabase
{
    public class RepaymentProfile : Profile
    {
        public RepaymentProfile()
        {
            // Database to Entity
            CreateMap<RepaymentEntity, Repayment>()
                .ForMember(d => d.BelongsTo, src => src.MapFrom(s => s.Group != null ? DatabaseMapper.Instance.Map<Group>(s.Group) : null))
                .ForMember(d => d.Sender, src => src.MapFrom(s => s.Sender != null ? DatabaseMapper.Instance.Map<User>(s.Sender) : null))
                .ForMember(d => d.Recipient, src => src.MapFrom(s => s.Recipient != null ? DatabaseMapper.Instance.Map<User>(s.Recipient) : null))
                .ForMember(d => d.RecordedBy, src => src.MapFrom(s => s.RecordedBy != null ? DatabaseMapper.Instance.Map<User>(s.RecordedBy) : null));

            // Entity to Database
            CreateMap<Repayment, RepaymentEntity>()
                .ForMember(d => d.Group, src => src.Ignore())
                .ForMember(d => d.Sender, src => src.Ignore())
                .ForMember(d => d.Recipient, src => src.Ignore())
                .ForMember(d => d.RecordedBy, src => src.Ignore())
                .ForMember(d => d.CreatedAt, src => src.Ignore())
                .ForMember(d => d.GroupId, src => src.MapFrom(s => s.BelongsTo != null ? s.BelongsTo.Id : 0))
                .ForMember(d => d.SenderUserId, src => src.MapFrom(s => s.Sender != null ? s.Sender.Id : 0))
                .ForMember(d => d.RecipientUserId, src => src.MapFrom(s => s.Recipient != null ? s.Recipient.Id : 0))
                .ForMember(d => d.RecordedByUserId, src => src.MapFrom(s => s.RecordedBy != null ? s.RecordedBy.Id : 0));
        }
    }
}
