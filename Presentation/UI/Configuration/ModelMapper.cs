using System;
using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;

namespace UI.Configuration
{
    public static class ModelMapper
    {
        public static IMapper Instance = GetMapperInstance();

        public static IMapper GetMapperInstance()
        {
            return
                new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile(Activator.CreateInstance(typeof(GroupProfile)) as Profile);
                    cfg.AddProfile(Activator.CreateInstance(typeof(UserProfile)) as Profile);
                    cfg.AddProfile(Activator.CreateInstance(typeof(ExpenseProfile)) as Profile);
                }, NullLoggerFactory.Instance).CreateMapper();
        }
    }
}
