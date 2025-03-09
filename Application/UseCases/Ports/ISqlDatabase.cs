using System;
using Core.Entities;

namespace Application.UseCases.Ports
{
   public interface ISqlDatabase<T> where T : class
   {
      Task<T?> GetAsync(T entity);
      Task<T?> CreateAsync(T entity);
      Task<T?> DeleteAsync(T entity);
      Task<T?> UpdateAsync(T entity);
   }
}