using System;
using AutoMapper;

namespace Infrastructure.SqlDatabase
{
    public static class DatabaseMapper
    {
        public static IMapper Instance = GetMapper();

        private static IMapper GetMapper()
        {
            return
                new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile(Activator.CreateInstance(typeof(UserProfile)) as Profile);
                    cfg.AddProfile(Activator.CreateInstance(typeof(UserProfile)) as Profile);
                    cfg.AddProfile(Activator.CreateInstance(typeof(GroupProfile)) as Profile);
                    cfg.AddProfile(Activator.CreateInstance(typeof(MemberOfProfile)) as Profile);
                    cfg.AddProfile(Activator.CreateInstance(typeof(ExpenseProfile)) as Profile);
                    cfg.AddProfile(Activator.CreateInstance(typeof(SplitProfile)) as Profile);
                    cfg.AddProfile(Activator.CreateInstance(typeof(PasswordProfile)) as Profile);
                }).CreateMapper();
        }
    }
}
