

using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class UsersController : BaseApiController
    {
        private readonly IUserRespository _userRespository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        public UsersController(IUserRespository userRespository, IMapper mapper, IPhotoService photoService)
        {
            _photoService = photoService;
            _mapper = mapper;
            _userRespository = userRespository;


        }
        [HttpGet]

        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
        {
            var user = await _userRespository.GetUserByUserNameAsync(User.GetUsername());
            userParams.CurrentUsername = user.UserName;
            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = user.Gender == "male" ? "female" : "male";
            }
            var users = await _userRespository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            return Ok(users);
        }
        [HttpGet("{username}", Name = "GetUser")]

        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var user = await _userRespository.GetUserByUserNameAsync(username);
            return _mapper.Map<MemberDto>(user);
        }
        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var username = User.GetUsername();
            var user = await _userRespository.GetUserByUserNameAsync(username);
            _mapper.Map(memberUpdateDto, user);
            _userRespository.Update(user);
            if (await _userRespository.SaveAllAsync())
            {
                return NoContent();
            }
            return BadRequest();


        }
        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var username = User.GetUsername();
            var user = await _userRespository.GetUserByUserNameAsync(username);
            var result = await _photoService.AddPhotoAsync(file);
            if (result.Error != null)
            {
                return BadRequest(result.Error.Message);
            }
            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId

            };
            if (user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }
            user.Photos.Add(photo);
            if (await _userRespository.SaveAllAsync())
            {
                return CreatedAtRoute("GetUser", new { username = user.UserName }, _mapper.Map<PhotoDto>(photo));
            }
            return BadRequest("Problem adding photo");
        }
        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var username = User.GetUsername();
            var user = await _userRespository.GetUserByUserNameAsync(username);
            var photo = user.Photos.FirstOrDefault(photo => photo.Id == photoId);
            if (photo.IsMain)
            {
                return BadRequest("This is already your main photo");
            }
            var currentMain = user.Photos.FirstOrDefault(photo => photo.IsMain);
            if (currentMain != null)
            {
                currentMain.IsMain = false;
            }
            photo.IsMain = true;
            if (await _userRespository.SaveAllAsync())
            {
                return NoContent();
            }
            return BadRequest("Failed to set main photo");
        }
        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var username = User.GetUsername();
            var user = await _userRespository.GetUserByUserNameAsync(username);
            var photo = user.Photos.FirstOrDefault(photo => photo.Id == photoId);
            if (photo == null)
            {
                return NotFound();
            }
            else if (photo.IsMain)
            {
                return BadRequest("You can not delete your main photo");
            }
            else if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null)
                {
                    return BadRequest(result.Error);
                }
            }
            user.Photos.Remove(photo);
            if (await _userRespository.SaveAllAsync())
            {
                return NoContent();

            }
            return BadRequest("Problem to remove photo");


        }

    }

}