using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRespository : IMessageRespository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public MessageRespository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.Include(m => m.Sender).Include(m => m.Recipient).SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var querry = _context.Messages.OrderByDescending(m => m.MessageSent).AsQueryable();
            querry = messageParams.Container switch
            {
                "Inbox" => querry.Where(u => u.Recipient.UserName == messageParams.Username && u.RecipientDeleted == false),
                "Outbox" => querry.Where(u => u.Sender.UserName == messageParams.Username && u.SenderDeleted == false),
                _ => querry.Where(u => u.Recipient.UserName == messageParams.Username && u.DateRead == null && u.RecipientDeleted == false)

            };
            return await PagedList<MessageDto>.CreateAsync(querry.ProjectTo<MessageDto>(_mapper.ConfigurationProvider), messageParams.PageNumber, messageParams.PageSize);


        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUsername)
        {
            var messages = await _context.Messages.Include(m => m.Sender).ThenInclude(p => p.Photos)
            .Include(u => u.Recipient).ThenInclude(p => p.Photos)
            .Where(m => m.Recipient.UserName == currentUserName && m.RecipientDeleted == false && m.Sender.UserName == recipientUsername ||
             m.Recipient.UserName == recipientUsername && m.Sender.UserName == currentUserName && m.SenderDeleted == false).OrderBy(m => m.MessageSent).ToListAsync();
            var unreadMessages = messages.Where(m => m.DateRead == null
            && m.Recipient.UserName == currentUserName).ToList();
            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.Now;
                }
                await _context.SaveChangesAsync();
            }
            return _mapper.Map<IEnumerable<MessageDto>>(messages);


        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}