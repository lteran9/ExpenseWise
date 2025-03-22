using System;
using Application.UseCases.Ports;
using Core.Entities;

namespace Infrastructure.SqlDatabase
{
   public interface IRepository : ISqlDatabase<User>
   {

   }
}