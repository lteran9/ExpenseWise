using System;

namespace Infrastructure.SqlDatabase
{
   public interface IQuery<T> where T : class
   {
      Task<List<T>> Find(T entity);
   }
}