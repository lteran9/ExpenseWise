using System;
using System.Threading.Tasks;
using Core.Entities;

namespace Application.UseCases.Ports
{
    public interface IGroupRepository
    {
        Task<Group?> FindByIdAsync(int id);
        Task<List<Group>?> ListAsync(Guid userKey);
        Task<Group?> FindByUniqueKeyAsync(Guid uniqueKey);
        Task<Group?> CreateAsync(Group group);
        Task<Group?> CreateWithOwnerMembershipAsync(Group group);
        Task<Group?> UpdateAsync(Group group);
        Task<Group?> DeleteAsync(Group group);
        Task<MemberOf?> AddMemberAsync(MemberOf member);
        /// <exception cref="GroupMembershipConflictException">
        /// The member has expense or repayment history at write time.
        /// </exception>
        Task<MemberOf?> RemoveMemberAsync(MemberOf member);
    }
}