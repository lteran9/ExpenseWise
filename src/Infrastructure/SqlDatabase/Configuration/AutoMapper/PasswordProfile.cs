using System;
using AutoMapper;
using Core.Entities;

namespace Infrastructure.SqlDatabase
{
    public class PasswordProfile : Profile
    {
        public PasswordProfile()
        {
            // Database to Entity
            CreateMap<PasswordEntity, Password>();

            // Entity to Database
            CreateMap<Password, PasswordEntity>();
        }
    }
}
