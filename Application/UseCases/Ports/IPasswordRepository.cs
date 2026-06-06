using System;
using System.Threading.Tasks;
using Core.Entities;

namespace Application.UseCases.Ports
{
    public interface IPasswordRepository
    {
        Task<Password?> FindByUserIdAsync(int userId);
        Task<Password?> CreateAsync(Password password);
        Task<Password?> UpdateAsync(Password password);
    }
}