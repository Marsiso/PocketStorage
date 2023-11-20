using AutoMapper;
using PocketStorage.Core.Application.Commands;
using PocketStorage.Domain.Application.Models;

namespace PocketStorage.Application.Application.Mappings;

public class UserProfile : Profile
{
    public UserProfile() => CreateMap<User, SignUpCommand>().ReverseMap();
}
