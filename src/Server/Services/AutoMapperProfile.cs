using AutoMapper;
using Vetrina.Server.Domain;
using Vetrina.Shared.Models;

namespace Vetrina.Server.Services
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserDTO>();

            CreateMap<User, LoginResponse>();

            CreateMap<RegisterUserRequest, User>();

            CreateMap<CreateUserRequest, User>();

            CreateMap<UpdateUserRequest, User>();
        }
    }
}