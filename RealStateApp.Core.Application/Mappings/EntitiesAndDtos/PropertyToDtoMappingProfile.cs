using AutoMapper;
using RealStateApp.Core.Application.Dtos.Properties;
using RealStateApp.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealStateApp.Core.Application.Mappings.EntitiesAndDtos
{
    public class PropertyToDtoMappingProfile : Profile
    {
        public PropertyToDtoMappingProfile()
        {
            CreateMap<Property, PropertyDto>();
            CreateMap<PropertyDto, Property>()
                .ForMember(dest => dest.PropertyType, opt => opt.Ignore())
                .ForMember(dest => dest.SaleType, opt => opt.Ignore())
                .ForMember(dest => dest.PropertyImprovements, opt => opt.Ignore())
                .ForMember(dest => dest.Photos, opt => opt.Ignore())
                .ForMember(dest => dest.FavoriteProperties, opt => opt.Ignore())
                .ForMember(dest => dest.Messages, opt => opt.Ignore())
                .ForMember(dest => dest.Offers, opt => opt.Ignore());




            CreateMap<Property, DataPropertyDto>()
                .ForMember(s => s.Photo, opt => opt.Ignore())
                .ForMember(s => s.SaleType, opt => opt.Ignore())
                .ForMember(s => s.TypeProperty, opt => opt.Ignore());

            CreateMap<Property, DetailsPropertyDto>()
                .ForMember(s => s.Images, opt => opt.Ignore())
                .ForMember(s => s.Name, opt => opt.Ignore())
                .ForMember(s => s.Email, opt => opt.Ignore())
                .ForMember(s => s.UrlImage, opt => opt.Ignore())
                .ForMember(s => s.Inproments, opt => opt.Ignore())
                .ForMember(s => s.PhoneNumber, opt => opt.Ignore())
                .ForMember(s => s.SaleType, opt => opt.Ignore())
                .ForMember(s => s.PropertyType, opt => opt.Ignore());

            // Mapping para la API según especificación del documento
            CreateMap<Property, PropertyApiDto>()
                .ForMember(dest => dest.PropertyType, opt => opt.MapFrom(src => src.PropertyType != null ? src.PropertyType.Name : "Unknown"))
                .ForMember(dest => dest.SaleType, opt => opt.MapFrom(src => src.SaleType != null ? src.SaleType.Name : "Unknown"))
                .ForMember(dest => dest.Improvements, opt => opt.MapFrom(src =>
                    src.PropertyImprovements != null && src.PropertyImprovements.Any()
                        ? src.PropertyImprovements
                            .Where(pi => pi.Improvement != null)
                            .Select(pi => pi.Improvement.Name)
                            .ToList()
                        : new List<string>()))
                .ForMember(dest => dest.AgentName, opt => opt.Ignore())
                .ForMember(dest => dest.AgentId, opt => opt.MapFrom(src => src.AgentId));

        }
    }
}
