using API.Entities;

namespace API
{
    public interface ITokenService
    {
        string createToken(AppUser user);
    }
}