using AutoMapper;
using OpenAPI;
using UI.Models;

namespace UI.Configuration;

public sealed class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<FindUserResponse, UserViewModel>()
            .ForMember(viewModel => viewModel.Id, options => options.MapFrom(user => user.UniqueKey));
    }
}
