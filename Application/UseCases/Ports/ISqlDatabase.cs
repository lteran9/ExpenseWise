using System;
using Core.Entities;

namespace Application.UseCases.Ports
{
   public interface ISqlDatabase<T> where T : class
   {
      Task<T?> Get(T entity);
      Task<T?> Create(T entity);
      Task<T?> Delete(T entity);
      Task<T?> Update(T entity);
   }
}