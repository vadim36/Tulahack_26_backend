using backend.Models.Auth;

namespace backend.Services.Token
{
    public interface ITokenService
    {
        Task<string> GenerateAccessToken(User user);
        Task<string> GenerateRefreshToken();
    }
}
