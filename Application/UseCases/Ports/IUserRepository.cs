using System;
using System.Threading.Tasks;
using Core.Entities;

namespace Application.UseCases.Ports
{
    public interface IUserRepository
    {
        Task<User?> FindByEmailAsync(string email);
        Task<User?> FindByPhoneAsync(string phone);
        Task<User?> FindByUniqueKeyAsync(Guid key);
        Task<User?> CreateAsync(User entity);
        Task<User?> UpdateAsync(User entity);
        Task<User?> DeleteAsync(User entity);
    }
}