using AutoMapper;
using RealStateApp.Core.Application.Dtos.Offer;
using RealStateApp.Core.Application.Interfaces;
using RealStateApp.Core.Domain.Entities;
using RealStateApp.Core.Domain.Interfaces;

namespace RealStateApp.Core.Application.Services
{
    public class OfferService : GenericService<Offer, CreateOfferDto>, IOfferService
    {

        private readonly IMapper _mapper;
        private readonly IOfferRepository offerRepository;



        public OfferService(IOfferRepository repo, IMapper mapper) : base(repo, mapper)
        {

            offerRepository = repo;
            _mapper = mapper;

        }




        public override async Task<CreateOfferDto?> AddAsync(CreateOfferDto? entityDto)
        {
            try
            {


                entityDto!.Id = 0;
                var entity = _mapper.Map<Offer>(entityDto);
                var property =  await offerRepository.AddAsync(entity);
                var dto = _mapper.Map<CreateOfferDto>(property);
                return dto;

            }
            catch
            {
                return default;
            }
        }





        public async Task<List<DataListOfferDto>> GetListByIdClientAndPropertyIdAsync(string ClientId, int propertyId)
        {
            try
            {

                var offers = await offerRepository.GetListByIdClientAndPropertyIdAsync(ClientId,propertyId);

                if(offers == null || !offers.Any()) 
                {

                    return new List<DataListOfferDto>();
                
                }


                var dto = _mapper.Map<List<DataListOfferDto>>(offers);
                return dto;




            }
            catch (Exception)
            {
                return new List<DataListOfferDto>();
            
            
            }


        }



        public async Task<bool> IsOfferActive(string clientId, int PropertyId)
        {


            bool DisableButton = false;
            var entities = await offerRepository.GetListByPropertyIdAsync(PropertyId);
            var clientOffers = await offerRepository.GetListByIdClientAndPropertyIdAsync(clientId, PropertyId);
            bool existOfferAcepceted = entities.Where(s => s.Status == Domain.Common.Enums.OfferStatus.Accepted).Any();
            bool existOfferClientPending = clientOffers.Where(s => s.Status == Domain.Common.Enums.OfferStatus.Pending).Any();

            if (existOfferAcepceted || existOfferClientPending)
            {

                DisableButton = true;

            }

            return DisableButton;   

        }



        public override async Task DeleteAsync(int id)
        {
            try
            {

                var entity = await offerRepository.GetByIdAsync(id);

                if (entity != null)
                {

                    await offerRepository.DeleteAsync(entity.Id);
                    return;

                }


                throw new Exception();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }




    }
}
