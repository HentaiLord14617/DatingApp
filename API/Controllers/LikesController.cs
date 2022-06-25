using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
        private readonly IUserRespository _userRespository;
        private readonly ILikeRespository _likeRespository;
        public LikesController(IUserRespository userRespository, ILikeRespository likeRespository)
        {
            _likeRespository = likeRespository;
            _userRespository = userRespository;
        }
        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId = User.GetUserId();
            var likedUser = await _userRespository.GetUserByUserNameAsync(username);
            var sourceUser = await _likeRespository.GetUserWithLikes(sourceUserId);
            if (likedUser == null)
            {
                return NotFound();
            }
            if (sourceUser.UserName == username)
            {
                return BadRequest("You cannot like yourself");
            }
            var userLike = await _likeRespository.GetUserLike(sourceUserId, likedUser.Id);
            if (userLike != null)
            {
                return BadRequest("You already like this user");
            }
            userLike = new UserLike
            {
                SourceUserId = sourceUserId,
                LikedUserId = likedUser.Id

            };
            sourceUser.LikedUser.Add(userLike);
            if (await _userRespository.SaveAllAsync())
            {
                return Ok();
            }
            return BadRequest("Failed to like user");
        }
        [HttpGet]
        public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery] LikeParams likeParams)
        {
            likeParams.userId = User.GetUserId();
            var user = await _likeRespository.GetUserLikes(likeParams);
            Response.AddPaginationHeader(user.CurrentPage, user.PageSize, user.TotalCount, user.TotalPages);
            return Ok(user);

        }

    }
}