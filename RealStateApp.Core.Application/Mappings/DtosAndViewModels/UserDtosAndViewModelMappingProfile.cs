using AutoMapper;
using RealStateApp.Core.Application.Dtos.User;
using RealStateApp.Core.Application.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace RealStateApp.Core.Application.Mappings.DtosAndViewModels
{
    public class UserDtosAndViewModelMappingProfile : Profile
    {

        public UserDtosAndViewModelMappingProfile()
        {
            CreateMap<UserDto, UserViewModel>();
            CreateMap<SaveBasicUserViewModel, SaveUserDto>();
            CreateMap<UserDto, SaveBasicUserViewModel>();

            // Mapeo específico para RegisterViewModel sin Dni
            CreateMap<RegisterViewModel, SaveUserDto>()
                .ForMember(dest => dest.Dni, opt => opt.MapFrom(src => "N/A"));

            CreateMap<AgentDto, AgentViewModel>();
            CreateMap<UserDto, AgentDataDto>()
                .ReverseMap();

        }
    }
}
