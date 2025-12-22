using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RealStateApp.Core.Application.Dtos.Properties;
using RealStateApp.Core.Application.Interfaces;
using RealStateApp.Core.Domain.Interfaces;

namespace RealStateApp.Core.Application.Features.Property.Queries.GetAllWithInclude
{
    public class GetAllPropertiesWithIncludeQuery : IRequest<IList<PropertyApiDto>>
    {
    }

    public class GetAllPropertiesWithIncludeQueryHandler : IRequestHandler<GetAllPropertiesWithIncludeQuery, IList<PropertyApiDto>>
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public GetAllPropertiesWithIncludeQueryHandler(
            IPropertyRepository propertyRepository,
            IMapper mapper,
            IUserService userService)
        {
            _propertyRepository = propertyRepository;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<IList<PropertyApiDto>> Handle(GetAllPropertiesWithIncludeQuery request, CancellationToken cancellationToken)
        {
            var propertiesQuery = _propertyRepository.GetAllQueryWithInclude(new List<string>
            {
                "PropertyType",
                "SaleType",
                "PropertyImprovements.Improvement"
            });

            var properties = await propertiesQuery.ToListAsync(cancellationToken);

            var propertyDtos = new List<PropertyApiDto>();

            foreach (var property in properties)
            {
                var dto = _mapper.Map<PropertyApiDto>(property);

                // Obtener nombre del agente
                var agent = await _userService.GetById(property.AgentId);
                dto.AgentName = agent != null ? $"{agent.FirstName} {agent.LastName}" : "Unknown";

                propertyDtos.Add(dto);
            }

            return propertyDtos;
        }
    }
}
