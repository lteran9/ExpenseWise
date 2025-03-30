using System;
using Application.UseCases.Ports;
using Core.Entities;

namespace Infrastructure.SqlDatabase
{
   public interface IRepository<T> where T : class
   {
      Task<T?> CreateAsync(T entity);
      Task<T?> RetrieveAsync(T entity);
      Task<T?> UpdateAsync(T entity);
      Task<T?> DeleteAsync(T entity);

   }
}