using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RealStateApp.Core.Application.Dtos.FavoriteProperty;
using RealStateApp.Core.Application.Dtos.Properties;
using RealStateApp.Core.Application.Interfaces;
using RealStateApp.Core.Domain.Common.Enums;
using RealStateApp.Core.Domain.Entities;
using RealStateApp.Core.Domain.Interfaces;

namespace RealStateApp.Core.Application.Services
{
    public class FavoritePropertyService : GenericService<FavoriteProperty, CreateFavoritePropertyDto>, IFavoritePropertyService
    {


        private readonly IFavoritePropertyRepository favoritePropertyRepository;
        private readonly IPropertyRepository propertyRepository;
        private readonly IMapper mapper;




        public FavoritePropertyService(IFavoritePropertyRepository repo, IMapper mapper, IPropertyRepository propertyRepository) : base(repo, mapper)
        {

            this.favoritePropertyRepository = repo;
            this.propertyRepository = propertyRepository;
            this.mapper = mapper;


        }




        public override async Task<CreateFavoritePropertyDto?> AddAsync(CreateFavoritePropertyDto? entityDto)
        {
            try
            {
                if (entityDto == null)
                    return null;

                var FavoriteProperty = await favoritePropertyRepository.GetFavoriteByIdClientAndByIdProperty(entityDto.ClientId, entityDto.PropertyId);


                if (FavoriteProperty == null)
                {


                    var entity = mapper.Map<FavoriteProperty>(entityDto);
                    var favoriteProperty = await favoritePropertyRepository.AddAsync(entity);
                    var dto = mapper.Map<CreateFavoritePropertyDto>(favoriteProperty);
                    return dto;
                }

                return null;
            }
            catch (Exception)
            {
                return default;
            }
        }






        public override async Task DeleteAsync(int id)
        {
            try
            {

                var entity = await favoritePropertyRepository.GetByIdAsync(id);

                if (entity != null)
                {

                    await favoritePropertyRepository.DeleteAsync(entity.Id);
                    return;

                }



                throw new Exception();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }




        public async Task<List<DataPropertyDto>> GetListPropertyFavoriteAsync(string client)
        {
            try
            {

                var list = new List<DataPropertyDto>();
                var favorities = await favoritePropertyRepository.GetListFavoriteByIdClient(client);


                if(favorities == null || favorities.Count == 0)
                {
                    return new List<DataPropertyDto>(); 
                }




                var ListPropertyFavorite = await propertyRepository.GetAllQuery()
                  .Include(p => p.PropertyType)
                  .Include(p => p.SaleType)
                  .Include(p => p.Photos)
                  .Where(p => p.Status == PropertyStatus.Available)
                  .OrderByDescending(p => p.Id)
                  .ToListAsync();


                if(ListPropertyFavorite == null || ListPropertyFavorite.Count == 0)
                {

                    return new List<DataPropertyDto>();
                
                }



                foreach (var item in favorities)
                {

                    var entity = ListPropertyFavorite!.FirstOrDefault(s => s.Id == item.PropertyId);
                  

                    if (entity != null)
                    {

                        var dto = mapper.Map<DataPropertyDto>(entity);
                        dto.SaleType = entity.SaleType.Name;
                        dto.TypeProperty =entity.PropertyType.Name;
                        dto.FavoriteId = item.Id;
                        dto.IsFavorite = true;
                        dto.Photo = entity.Photos.FirstOrDefault(d => d.PropertyId == item.PropertyId)!.ImageUrl;

                        list.Add(dto);
                       
                    }
                }


                if(list.Count < 0 || list == null) {  return new List<DataPropertyDto>(); }

                return list;
            }
            catch (Exception)
            {
                return new List<DataPropertyDto>();
            
            
            }


        }
    }
}
