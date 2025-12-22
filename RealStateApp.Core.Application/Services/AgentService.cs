using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RealStateApp.Core.Application.Dtos.Properties;
using RealStateApp.Core.Application.Interfaces;
using RealStateApp.Core.Domain.Common.Enums;
using RealStateApp.Core.Domain.Entities;
using RealStateApp.Core.Domain.Interfaces;

namespace RealStateApp.Core.Application.Services
{
    public class AgentService : IAgentService
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IPropertyTypeRepository _propertyTypeRepository;
        private readonly ISaleTypeRepository _saleTypeRepository;
        private readonly IPropertyPhotoRepository _propertyPhotoRepository;
        private readonly IPropertyImprovementRepository _propertyImprovementRepository;
        private readonly IMapper _mapper;

        public AgentService(
            IPropertyRepository propertyRepository,
            IPropertyTypeRepository propertyTypeRepository,
            ISaleTypeRepository saleTypeRepository,
            IPropertyPhotoRepository propertyPhotoRepository,
            IPropertyImprovementRepository propertyImprovementRepository,
            IMapper mapper)
        {
            _propertyRepository = propertyRepository;
            _propertyTypeRepository = propertyTypeRepository;
            _saleTypeRepository = saleTypeRepository;
            _propertyPhotoRepository = propertyPhotoRepository;
            _propertyImprovementRepository = propertyImprovementRepository;
            _mapper = mapper;
        }

        public async Task<List<AgentPropertyDto>> GetAgentProperties(string agentId)
        {
            var properties = await _propertyRepository.GetAllQuery()
                .Include(p => p.PropertyType)
                .Include(p => p.SaleType)
                .Include(p => p.Photos)
                .Where(p => p.AgentId == agentId)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            var result = new List<AgentPropertyDto>();

            foreach (var property in properties)
            {
                var dto = new AgentPropertyDto
                {
                    Id = property.Id,
                    Code = property.Code,
                    PropertyTypeName = property.PropertyType.Name,
                    SaleTypeName = property.SaleType.Name,
                    Price = property.Price,
                    SizeInMeters = property.SizeInMeters,
                    Bedrooms = property.Bedrooms,
                    Bathrooms = property.Bathrooms,
                    Status = property.Status,
                    MainPhoto = property.Photos.FirstOrDefault()?.ImageUrl
                };

                result.Add(dto);
            }

            return result;
        }

        public async Task<AgentPropertyDto?> GetPropertyById(int propertyId)
        {
            var property = await _propertyRepository.GetAllQuery()
                .Include(p => p.PropertyType)
                .Include(p => p.SaleType)
                .Include(p => p.Photos)
                .FirstOrDefaultAsync(p => p.Id == propertyId);

            if (property == null) return null;

            return new AgentPropertyDto
            {
                Id = property.Id,
                Code = property.Code,
                PropertyTypeName = property.PropertyType.Name,
                SaleTypeName = property.SaleType.Name,
                Price = property.Price,
                SizeInMeters = property.SizeInMeters,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                Status = property.Status,
                MainPhoto = property.Photos.FirstOrDefault()?.ImageUrl
            };
        }

        public async Task<SavePropertyDto> CreateProperty(SavePropertyDto dto, string agentId)
        {
            var code = await GenerateUniqueCode();

            var property = new Property
            {
                Code = code,
                PropertyTypeId = dto.PropertyTypeId,
                SaleTypeId = dto.SaleTypeId,
                Price = dto.Price,
                SizeInMeters = dto.SizeInMeters,
                Bedrooms = dto.Bedrooms,
                Bathrooms = dto.Bathrooms,
                Description = dto.Description,
                AgentId = agentId,
                Status = PropertyStatus.Available
            };

            await _propertyRepository.AddAsync(property);

            // Guardar mejoras
            if (dto.ImprovementIds != null && dto.ImprovementIds.Any())
            {
                foreach (var improvementId in dto.ImprovementIds)
                {
                    var propertyImprovement = new PropertyImprovement
                    {
                        PropertyId = property.Id,
                        ImprovementId = improvementId
                    };
                    await _propertyImprovementRepository.AddAsync(propertyImprovement);
                }
            }

            dto.Id = property.Id;
            return dto;
        }

        public async Task UpdateProperty(SavePropertyDto dto)
        {
            if (!dto.Id.HasValue)
                throw new Exception("El Id de la propiedad es requerido para actualizar");
            
            var property = await _propertyRepository.GetByIdAsync(dto.Id.Value);
            
            if (property == null)
                throw new Exception("Propiedad no encontrada");

            property.PropertyTypeId = dto.PropertyTypeId;
            property.SaleTypeId = dto.SaleTypeId;
            property.Price = dto.Price;
            property.SizeInMeters = dto.SizeInMeters;
            property.Bedrooms = dto.Bedrooms;
            property.Bathrooms = dto.Bathrooms;
            property.Description = dto.Description;

            await _propertyRepository.UpdateAsync(property.Id, property);

            // Actualizar mejoras - eliminar las existentes
            await _propertyImprovementRepository.DeleteByPropertyId(property.Id);

            if (dto.ImprovementIds != null && dto.ImprovementIds.Any())
            {
                foreach (var improvementId in dto.ImprovementIds)
                {
                    var propertyImprovement = new PropertyImprovement
                    {
                        PropertyId = property.Id,
                        ImprovementId = improvementId
                    };
                    await _propertyImprovementRepository.AddAsync(propertyImprovement);
                }
            }
        }

        public async Task DeleteProperty(int propertyId)
        {
            await _propertyRepository.DeleteAsync(propertyId);
        }

        public async Task<string> GenerateUniqueCode()
        {
            string code;
            bool exists;

            do
            {
                code = GenerateRandomCode();
                var property = await _propertyRepository.GetByCode(code);
                exists = property != null;
            } while (exists);

            return code;
        }

        private string GenerateRandomCode()
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
