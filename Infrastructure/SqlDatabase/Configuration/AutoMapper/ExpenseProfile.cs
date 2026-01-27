using System;
using AutoMapper;
using Core.Entities;

namespace Infrastructure.SqlDatabase
{
    public class ExpenseProfile : Profile
    {
        public ExpenseProfile()
        {
            // Database to Entity
            CreateMap<ExpenseEntity, Expense>();

            // Entity to Database
            CreateMap<Expense, ExpenseEntity>();
        }
    }
}
