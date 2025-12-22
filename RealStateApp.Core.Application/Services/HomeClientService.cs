using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RealStateApp.Core.Application.Dtos.Properties;
using RealStateApp.Core.Application.Dtos.User;
using RealStateApp.Core.Application.Interfaces;
using RealStateApp.Core.Domain.Common.Enums;
using RealStateApp.Core.Domain.Entities;
using RealStateApp.Core.Domain.Interfaces;


namespace RealStateApp.Core.Application.Services
{
    public class HomeClientService : GenericService<Property, PropertyDto>, IHomeClienteService
    {
        private readonly IMapper _mapper;
        private readonly IPropertyRepository propertyRepository;
        private readonly IPropertyTypeRepository propertyTypeRepository;
        private readonly ISaleTypeRepository _saleTypeRepository;
        private readonly IImprovementRepository improvementRepository;
        private readonly IAccountServiceForWebApp accountServiceForWebApp;
        private readonly IPropertyPhotoRepository propertyPhotoRepository;
        private readonly IPropertyImprovementRepository propertyImprovementRepository;
        private readonly IFavoritePropertyRepository favoritePropertyRepository;
        private readonly IUserService userService;

        public HomeClientService(
                                IMapper mapper,
                                IPropertyRepository propertyRepository,
                                IPropertyTypeRepository propertyTypeRepository,
                                ISaleTypeRepository _saleTypeRepository,
                                IPropertyPhotoRepository propertyPhotoRepository,
                                IAccountServiceForWebApp accountServiceForWebApp,
                                IImprovementRepository improvementRepository,
                                IPropertyImprovementRepository propertyImprovementRepository,
                                IFavoritePropertyRepository favoritePropertyRepository,
                                 IUserService userService
            )
                                : base(propertyRepository, mapper)
        {

            this.propertyRepository = propertyRepository;
            this.propertyTypeRepository = propertyTypeRepository;
            this._saleTypeRepository = _saleTypeRepository;
            this.propertyPhotoRepository = propertyPhotoRepository;
            this.improvementRepository = improvementRepository;
            this.accountServiceForWebApp = accountServiceForWebApp;
            this.favoritePropertyRepository = favoritePropertyRepository;
            this.propertyImprovementRepository = propertyImprovementRepository;
            _mapper = mapper;
            this.userService = userService;
        }






        public async Task<List<DataPropertyDto>> ListProperty(string clientId)
        {
            try
            {

                var properties = await propertyRepository.GetAllQuery()
                    .Include(p => p.PropertyType)
                    .Include(p => p.SaleType)
                    .Include(p => p.Photos)
                    .Where(p => p.Status == PropertyStatus.Available)
                    .OrderByDescending(p => p.Id)
                    .ToListAsync();

                if (!properties.Any())
                    return new List<DataPropertyDto>();

                var favorites = await favoritePropertyRepository.GetAllQuery()
                    .Where(f => f.ClientId == clientId)
                    .ToListAsync();

                var result = new List<DataPropertyDto>();

                foreach (var p in properties)
                {
                    var fav = favorites.FirstOrDefault(f => f.PropertyId == p.Id);

                    var dto = new DataPropertyDto
                    {
                        Id = p.Id,
                        Bedrooms = p.Bedrooms,
                        Bathrooms = p.Bathrooms,
                        Code = p.Code,
                        SizeInMeters = p.SizeInMeters,
                        Price = p.Price,
                        SaleType = p.SaleType.Name,
                        TypeProperty = p.PropertyType.Name,
                        Photo = p.Photos.FirstOrDefault()?.ImageUrl ?? "",
                        IsFavorite = fav != null,
                        FavoriteId = fav?.Id ?? 0
                    };

                    result.Add(dto);
                }

                return result;
            }
            catch
            {
                return new List<DataPropertyDto>();
            }
        }






        public async Task<DetailsPropertyDto?> ListDetailProperty(int propertyId)
        {
            try
            {


                var property = await propertyRepository.GetByIdAsync(propertyId);


                if (property == null)
                {

                    return null;

                }


                var URLs = new List<string>();
                var _NameImprovements = new List<string>();


                var SaleType = await _saleTypeRepository.GetByIdAsync(property.SaleTypeId);
                var propertyType = await propertyTypeRepository.GetByIdAsync(property.PropertyTypeId);
                var agent = await accountServiceForWebApp.GetById(property.AgentId);
                var Propertyphotos = await propertyPhotoRepository.GetListPhotByPropertyId(propertyId);
                var propertyImmprovement = await propertyImprovementRepository.GetPropertyImprovementByPropertyId(propertyId);
                var ImprovementList = await propertyImprovementRepository.GetListImprovementBYPorpertyId(propertyId);



                foreach (var item in ImprovementList)
                {

                    var entity = await improvementRepository.GetByIdAsync(item.ImprovementId);

                    if (entity == null)
                    {
                        continue;

                    }

                    _NameImprovements.Add(entity!.Name);

                }


                foreach (var photo in Propertyphotos)
                {

                    URLs.Add(photo.ImageUrl);
                }






                if (SaleType != null && propertyType != null)
                {

                    var Data = _mapper.Map<DetailsPropertyDto>(property);
                    Data.Images = URLs;
                    Data.Inproments = _NameImprovements;
                    Data.SaleType = SaleType.Name;
                    Data.PropertyType = propertyType.Name;
                    //Data agent
                    Data.Name = agent!.FirstName;
                    Data.Email = agent!.Email;
                    Data.UrlImage = agent.Photo ?? "";
                    Data.PhoneNumber = agent.PhoneNumber ?? "";
                    return Data;

                }

                return null;
            }
            catch (Exception)
            {
                return null;



            }

        }



        public async Task<DataPropertyDto?> FilterByCode(string? code = null, string? userId = null)
        {
            var property = await propertyRepository.GetByCode(code ?? "");

            if (property == null)
                return null;

            var saleType = await _saleTypeRepository.GetByIdAsync(property.SaleTypeId);
            var propertyType = await propertyTypeRepository.GetByIdAsync(property.PropertyTypeId);
            var photo = await propertyPhotoRepository.GetPhotoByPropertyId(property.Id);

            if (saleType == null || propertyType == null)
                return null;

            var dto = _mapper.Map<DataPropertyDto>(property);

            dto.Photo = photo?.ImageUrl ?? "";
            dto.SaleType = saleType.Name;
            dto.TypeProperty = propertyType.Name;


            if (!string.IsNullOrWhiteSpace(userId))
            {
                var fav = await favoritePropertyRepository.GetFavoriteByIdClientAndByIdProperty(userId, property.Id);

                if (fav != null)
                {
                    dto.IsFavorite = true;
                    dto.FavoriteId = fav.Id;
                }
            }

            return dto;
        }






        public async Task<List<DataPropertyDto>> FilterMultiple(string? propertyType, decimal? priceMIN, decimal? priceMax, int? Bedrooms, int? Bathrooms, string? userId)
        {
            var query = propertyRepository
                .GetAllQuery()
                .Where(p => p.Status == PropertyStatus.Available);


            if (!string.IsNullOrWhiteSpace(propertyType))
                query = query.Where(p => p.PropertyType.Name == propertyType);

            if (priceMIN.HasValue)
                query = query.Where(p => p.Price >= priceMIN.Value);

            if (priceMax.HasValue)
                query = query.Where(p => p.Price <= priceMax.Value);

            if (Bedrooms.HasValue)
                query = query.Where(p => p.Bedrooms == Bedrooms.Value);

            if (Bathrooms.HasValue)
                query = query.Where(p => p.Bathrooms == Bathrooms.Value);



            var result = await query
                .Include(p => p.PropertyType)
                .Include(p => p.SaleType)
                .Include(p => p.Photos)
                .OrderByDescending(p => p.Id)
                .Select(p => new DataPropertyDto
                {
                    Id = p.Id,
                    Bathrooms = p.Bathrooms,
                    Bedrooms = p.Bedrooms,
                    Code = p.Code,
                    Photo = p.Photos.FirstOrDefault()!.ImageUrl ?? "",
                    Price = p.Price,
                    SaleType = p.SaleType.Name,
                    SizeInMeters = p.SizeInMeters,
                    TypeProperty = p.PropertyType.Name,
                })
                .ToListAsync();



            if (!string.IsNullOrWhiteSpace(userId))
            {
                var favorites = await favoritePropertyRepository.GetListFavoriteByIdClient(userId);
                var favIds = favorites!.Select(f => f.PropertyId).ToHashSet();

                foreach (var item in result)
                {
                    if (favIds.Contains(item.Id))
                    {
                        item.IsFavorite = true;
                        item.FavoriteId = favorites!
                            .First(f => f.PropertyId == item.Id).Id;
                    }
                }
            }

            return result;
        }

        public async Task<List<AgentDataDto>> ListAgentAsync()
        {


            try
            {

                var entities = await userService.GetUsersAgentnOnly();
                var ListEntities = new List<AgentDataDto>();

                var AgentActive = entities.Where(s => s.IsActive == true).ToList();

                if (AgentActive.Count < 0 && !AgentActive.Any())
                {


                    return new List<AgentDataDto>();

                }


                foreach (var entity in AgentActive)
                {

                    var agent = _mapper.Map<AgentDataDto>(entity);
                    agent.UrlImage = entity.Photo ?? "";

                    ListEntities.Add(agent);

                }


                return ListEntities;


            }
            catch (Exception)
            {
                return new List<AgentDataDto>();
            }

        }




        public async Task<List<DataPropertyDto>> ListPropertyAgentAsync(string Agent, string clientId)
        {
            try
            {



                var ListPropertyAgent = await propertyRepository.GetAllQuery()
                  .Include(p => p.PropertyType)
                  .Include(p => p.SaleType)
                  .Include(p => p.Photos)
                  .Where(p => p.Status == PropertyStatus.Available)
                  .OrderByDescending(p => p.Id)
                  .ToListAsync();


                var properties = ListPropertyAgent.Where(s => s.AgentId == Agent).ToList();


         
                if (!properties.Any())
                    return new List<DataPropertyDto>();

                var favorites = await favoritePropertyRepository.GetAllQuery()
                    .Where(f => f.ClientId == clientId)
                    .ToListAsync();

                var result = new List<DataPropertyDto>();

                foreach (var p in properties)
                {
                    var fav = favorites.FirstOrDefault(f => f.PropertyId == p.Id);

                    var dto = new DataPropertyDto
                    {
                        Id = p.Id,
                        Bedrooms = p.Bedrooms,
                        Bathrooms = p.Bathrooms,
                        Code = p.Code ?? "",
                        SizeInMeters = p.SizeInMeters,
                        Price = p.Price,
                        SaleType = p.SaleType.Name ?? "",
                        TypeProperty = p.PropertyType.Name ?? "",
                        Photo = p.Photos.FirstOrDefault()?.ImageUrl ?? "",
                        IsFavorite = fav != null,
                        FavoriteId = fav?.Id ?? 0,
                        AgentId = p.AgentId ?? "",
                    };

                    result.Add(dto);
                }


                return result;
            }
            catch (Exception)
            {
                return new List<DataPropertyDto>();
            }
        }

        public async Task<List<AgentDataDto>> GetAgentByName(string Name)
        {
            try
            {

                var entities = await accountServiceForWebApp.GetByNameAsync(Name);
                var Agents = entities.Where( s => s.Role == AppRoles.AGENT.ToString() && s.IsActive == true).ToList();   
                var FilterAgent = new List<AgentDataDto>();




                if(Agents == null || Agents.Count == 0)
                {
                    return new List<AgentDataDto>();
                }


                foreach (var entity in Agents)
                {
                   var dto = _mapper.Map<AgentDataDto>(entity);
                   dto.UrlImage = entity.Photo ?? "";
                   FilterAgent.Add(dto);    
                }

                return FilterAgent;
            }
            catch (Exception)
            {
                return new List<AgentDataDto>();
            }
        }
    }



}
