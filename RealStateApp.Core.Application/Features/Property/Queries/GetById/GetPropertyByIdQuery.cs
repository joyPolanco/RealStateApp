using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RealStateApp.Core.Application.Dtos.Properties;
using RealStateApp.Core.Application.Exceptions;
using RealStateApp.Core.Application.Interfaces;
using RealStateApp.Core.Domain.Interfaces;
using System.Net;

namespace RealStateApp.Core.Application.Features.Property.Queries.GetById
{
    /// <summary>
    /// Query para obtener una propiedad por su Id
    /// Endpoint: GET /api/v1/properties/{id}
    /// Roles: Administrador, Desarrollador
    /// </summary>
    public class GetPropertyByIdQuery : IRequest<PropertyApiDto>
    {
        public required int Id { get; set; }
    }

    public class GetPropertyByIdQueryHandler : IRequestHandler<GetPropertyByIdQuery, PropertyApiDto>
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public GetPropertyByIdQueryHandler(
            IPropertyRepository propertyRepository, 
            IMapper mapper,
            IUserService userService)
        {
            _propertyRepository = propertyRepository;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<PropertyApiDto> Handle(GetPropertyByIdQuery request, CancellationToken cancellationToken)
        {
            var propertiesQuery = _propertyRepository.GetAllQueryWithInclude(new List<string>
            {
                "PropertyType",
                "SaleType",
                "PropertyImprovements.Improvement"
            });

            var property = await propertiesQuery
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (property == null)
                throw new ApiException("El id de la propiedad es inválido",(int)HttpStatusCode.NotFound);

            var dto = _mapper.Map<PropertyApiDto>(property);
            
            // Obtener el nombre del agente usando UserService
            var agent = await _userService.GetById(property.AgentId);
            dto.AgentName = agent != null ? $"{agent.FirstName} {agent.LastName}" : "Unknown";

            return dto;
        }
    }
}
