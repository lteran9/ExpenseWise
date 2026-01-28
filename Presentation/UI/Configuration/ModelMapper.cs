using System;
using AutoMapper;

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
                }).CreateMapper();
        }
    }
}
