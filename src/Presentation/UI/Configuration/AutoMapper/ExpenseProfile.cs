using System;
using Application.UseCases;
using AutoMapper;
using Core.Entities;
using UI.Models;

namespace UI.Configuration
{
    public class ExpenseProfile : Profile
    {
        public ExpenseProfile()
        {
            CreateMap<Expense, ExpenseViewModel>();
        }
    }
}
