using System.Security.Claims;

namespace DiscordLite_WEB.Services.IServices
{
    public interface ITokenProvider
    {
        void SetToken(string accessToken, string refreshToken);
        string? GetAccessToken();
        string? GetRefreshToken();
        ClaimsPrincipal? CreatePrincipalFromJWTToken(string token);
        void ClearToken();
    }
}