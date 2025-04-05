using System;
using Core.Entities;

namespace Application.UseCases.Ports
{
   public interface IDatabasePort<T> where T : class
   {
      Task<T?> CreateAsync(T entity);
      Task<T?> RetrieveAsync(T entity);
      Task<T?> DeleteAsync(T entity);
      Task<T?> UpdateAsync(T entity);
   }
}