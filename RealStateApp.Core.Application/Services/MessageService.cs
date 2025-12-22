

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RealStateApp.Core.Application.Dtos.Message;
using RealStateApp.Core.Application.Interfaces;
using RealStateApp.Core.Domain.Entities;
using RealStateApp.Core.Domain.Interfaces;

namespace RealStateApp.Core.Application.Services
{
    public class MessageService : GenericService<Message, CreateMessageDto>, IMessageService
    {

        private readonly IMessageRepository messageRepository;

        public MessageService(IMessageRepository repo, IMapper mapper) : base(repo, mapper)
        {

            messageRepository = repo;
        }



        public async Task<List<DataConversactionDto>> GetConversaction(int propertyId, string clientId, string agentId)
        {
            try
            {

                var messages = await messageRepository.GetAllQuery()
                .Where(m => m.PropertyId == propertyId &&
                (
                   (m.SenderUserId == clientId && m.ReceiverUserId == agentId) ||
                   (m.SenderUserId == agentId && m.ReceiverUserId == clientId)
                ))
               .OrderBy(m => m.Date)
               .Select(m => new DataConversactionDto
               {
                  Id = m.Id,
                  Content = m.Content,
                  Date = m.Date,
                  SenderUserId = m.SenderUserId,
                  ReceiverUserId = m.ReceiverUserId
               }).ToListAsync();

                return messages;
            }
            catch (Exception)
            {
                return new List<DataConversactionDto>();
            }
        }



        public override async Task DeleteAsync(int id)
        {
            try
            {

                var entity = await messageRepository.GetByIdAsync(id);

                if (entity != null)
                {

                    await messageRepository.DeleteAsync(entity.Id);
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
