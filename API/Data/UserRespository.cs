using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRespository : IUserRespository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public UserRespository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;

        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users.Include(p => p.Photos).ToListAsync();
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUserNameAsync(string username)
        {
            return await _context.Users.Include(p => p.Photos).SingleOrDefaultAsync(user => user.UserName == username);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var querry = _context.Users.AsQueryable();
            querry = querry.Where(u => u.UserName != userParams.CurrentUsername);
            querry = querry.Where(u => u.Gender == userParams.Gender);
            var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var maxDob = DateTime.Today.AddYears(-userParams.MinAge);
            querry = querry.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            querry = userParams.OrderBy switch
            {
                "created" => querry.OrderByDescending(u => u.Created),
                _ => querry.OrderByDescending(u => u.LastActive)
            };
            return await PagedList<MemberDto>.CreateAsync(querry.ProjectTo<MemberDto>(_mapper.ConfigurationProvider).AsNoTracking(), userParams.PageNumber, userParams.PageSize);
        }

        public Task<MemberDto> GetMemberAsync(string username)
        {
            throw new NotImplementedException();
        }
    }
}