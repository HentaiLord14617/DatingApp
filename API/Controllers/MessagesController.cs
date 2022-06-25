using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class MessagesController : BaseApiController
    {
        private readonly IMessageRespository _messageRespository;
        private readonly IUserRespository _userRespository;
        private readonly IMapper _mapper;
        public MessagesController(IUserRespository userRespository, IMessageRespository messageRespository, IMapper mapper)
        {
            _mapper = mapper;
            _userRespository = userRespository;
            _messageRespository = messageRespository;
        }
        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            Console.WriteLine("This is a test");
            var username = User.GetUsername();
            if (username == createMessageDto.RecipientUsername)
            {
                return BadRequest("You cannot send messages to yourself");
            }
            var sender = await _userRespository.GetUserByUserNameAsync(username);
            var recipient = await _userRespository.GetUserByUserNameAsync(createMessageDto.RecipientUsername);
            if (recipient == null)
            {
                return NotFound();
            }
            var message = new Message
            {
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Sender = sender,
                Recipient = recipient,
                Content = createMessageDto.Content
            };
            _messageRespository.AddMessage(message);
            if (await _messageRespository.SaveAllAsync())
            {
                return _mapper.Map<MessageDto>(message); ;
            }
            return BadRequest("Failed to send message");




        }
        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();
            var messages = await _messageRespository.GetMessagesForUser(messageParams);
            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);
            return messages;


        }
        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            var currentUserName = User.GetUsername();
            return Ok(await _messageRespository.GetMessageThread(currentUserName, username));
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUsername();
            var message = await _messageRespository.GetMessage(id);
            if (message.Sender.UserName != username && message.Recipient.UserName != username)
            {
                return Unauthorized();
            }
            if (message.Sender.UserName == username)
            {
                message.SenderDeleted = true;
            }
            if (message.Recipient.UserName == username)
            {
                message.RecipientDeleted = true;
            }
            if (message.SenderDeleted && message.RecipientDeleted)
            {
                _messageRespository.DeleteMessage(message);
            }
            if (await _messageRespository.SaveAllAsync())
            {
                return Ok();
            }
            return BadRequest("Problem deleting the message");
        }
    }
}