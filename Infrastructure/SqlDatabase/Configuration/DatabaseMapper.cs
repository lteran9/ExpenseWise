using System;
using AutoMapper;

namespace Infrastructure.SqlDatabase
{
    public static class DatabaseMapper
    {
        public static IMapper UserMapper
        {
            get
            {
                return
                   new MapperConfiguration(cfg =>
                   {
                       cfg.AddProfile(Activator.CreateInstance(typeof(UserProfile)) as Profile);
                   }).CreateMapper();
            }
        }

        public static IMapper GroupMapper
        {
            get
            {
                return
                   new MapperConfiguration(cfg =>
                   {
                       cfg.AddProfile(Activator.CreateInstance(typeof(GroupProfile)) as Profile);
                   }).CreateMapper();
            }
        }

        public static IMapper MemberOfMapper
        {
            get
            {
                return
                   new MapperConfiguration(cfg =>
                   {
                       cfg.AddProfile(Activator.CreateInstance(typeof(MemberOfProfile)) as Profile);
                   }).CreateMapper();
            }
        }

        public static IMapper ExpenseMapper
        {
            get
            {
                return
                   new MapperConfiguration(cfg =>
                   {
                       cfg.AddProfile(Activator.CreateInstance(typeof(ExpenseProfile)) as Profile);
                   }).CreateMapper();
            }
        }

        public static IMapper SplitMapper
        {
            get
            {
                return
                   new MapperConfiguration(cfg =>
                   {
                       cfg.AddProfile(Activator.CreateInstance(typeof(SplitProfile)) as Profile);
                   }).CreateMapper();
            }
        }

        public static IMapper PasswordMapper
        {
            get
            {
                return
                   new MapperConfiguration(cfg =>
                   {
                       cfg.AddProfile(Activator.CreateInstance(typeof(PasswordProfile)) as Profile);
                   }).CreateMapper();
            }
        }
    }
}
