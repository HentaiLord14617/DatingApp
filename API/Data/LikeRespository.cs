
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikeRespository : ILikeRespository
    {
        private readonly DataContext _context;
        public LikeRespository(DataContext context)
        {
            _context = context;
        }

        public async Task<UserLike> GetUserLike(int SourceUserId, int likedUserId)
        {
            return await _context.Likes.FindAsync(SourceUserId, likedUserId);
        }

        public async Task<PagedList<LikeDto>> GetUserLikes(LikeParams likeParams)
        {
            var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
            var likes = _context.Likes.AsQueryable();
            if (likeParams.predicate == "liked")
            {
                likes = likes.Where(like => like.SourceUserId == likeParams.userId);
                users = likes.Select(like => like.LikedUser);
            }
            if (likeParams.predicate == "likedBy")
            {
                likes = likes.Where(like => like.LikedUserId == likeParams.userId);
                users = likes.Select(like => like.SourceUser);
            }
            var likedUsers = users.Select(user => new LikeDto
            {
                Username = user.UserName,
                Age = user.GetAge(),
                KnownAs = user.KnownAs,
                PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain).Url,
                City = user.City,
                Id = user.Id
            });
            return await PagedList<LikeDto>.CreateAsync(likedUsers, likeParams.PageNumber, likeParams.PageSize);
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users.Include(x => x.LikedUser).FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
}