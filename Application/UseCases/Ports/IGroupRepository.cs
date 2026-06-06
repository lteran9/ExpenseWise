using System;
using System.Threading.Tasks;
using Core.Entities;

namespace Application.UseCases.Ports
{
    public interface IGroupRepository
    {
        Task<Group?> FindByIdAsync(int id);
        Task<Group?> FindByUniqueKeyAsync(Guid uniqueKey);
        Task<Group?> CreateAsync(Group group);
        Task<Group?> UpdateAsync(Group group);
        Task<Group?> DeleteAsync(Group group);
        Task<MemberOf?> AddMemberAsync(MemberOf member);
        Task<MemberOf?> RemoveMemberAsync(MemberOf member);
    }
}