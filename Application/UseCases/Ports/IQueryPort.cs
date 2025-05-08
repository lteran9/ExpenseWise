using System;

namespace Application.UseCases
{
   public interface IQueryPort<T> where T : class
   {
      Task<List<T>> FindAsync(T entity);
   }
}