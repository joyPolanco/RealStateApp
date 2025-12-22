using AutoMapper;
using MediatR;
using RealStateApp.Core.Application.Dtos.User;
using RealStateApp.Core.Application.Exceptions;
using RealStateApp.Core.Application.Interfaces;
using RealStateApp.Core.Domain.Interfaces;
using System.Net;
using System.Threading;
using System.Threading.Tasks;


namespace RealStateApp.Core.Application.Features.Agents.Queries.GetById
{
    /// <summary>
    /// Query para obtener un agente específico por su Id.
    /// </summary>
    /// <remarks>
    /// Ejemplo de uso:
    /// GET /api/v1/agents/{id}
    ///
    /// Respuesta exitosa:
    /// 200 OK
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174000",
    ///   "firstName": "Juan",
    ///   "lastName": "Pérez",
    ///   "email": "juan.perez@example.com",
    ///   "status": "Active",
    ///   "propertiesCount": 5
    /// }
    ///
    /// Respuesta si no se encuentra el agente:
    /// 404 Not Found
    /// {
    ///   "message": "Agent not found with this Id"
    /// }
    /// </remarks>
    public class GetAgentByIdQuery : IRequest<AgentDto>
    {
        /// <summary>
        /// Id del agente a consultar
        /// </summary>
        public string? Id { get; set; }
    }

    /// <summary>
    /// Handler para GetAgentByIdQuery
    /// </summary>
    public class GetAgentByIdQueryHandler : IRequestHandler<GetAgentByIdQuery, AgentDto>
    {
        private readonly IUserService _userService;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IMapper _mapper;

        public GetAgentByIdQueryHandler(
            IUserService userService,
            IPropertyRepository propertyRepository,
            IMapper mapper)
        {
            _userService = userService;
            _propertyRepository = propertyRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Ejecuta la query para devolver un agente con su información básica y número de propiedades.
        /// </summary>
        /// <param name="request">Query con el Id del agente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>AgentDto con los datos del agente</returns>
        public async Task<AgentDto> Handle(GetAgentByIdQuery request, CancellationToken cancellationToken)
        {
            // Obtener el agente desde el servicio de usuarios
            var userBase = await _userService.GetById(request.Id ?? "");


            if (userBase == null) 
                throw new ApiException($"Agent not found with this Id",(int)HttpStatusCode.NotFound);

            if (userBase == null)
                throw new ApiException("Agent not found with Id");


            // Obtener el conteo de propiedades del agente
            var propertiesCount = await _propertyRepository.GetAgentPropertiesCount(request.Id);

            // Mapear la entidad a DTO
            var dto = _mapper.Map<AgentDto>(userBase);
            dto.PropertiesCount = propertiesCount;

            return dto;
        }
    }
}
