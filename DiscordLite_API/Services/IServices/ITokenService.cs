using DiscordLite_API.Model;

namespace DiscordLite_API.Services.IServices
{
    public interface ITokenService
    {
        Task<string> GenerateJwtTokenAsync(User user);
        Task<string> GenerateRefreshTokenAsync();
        Task SaveRefreshTokenAsync(string userId, string jwtTokenId, string refreshToken, DateTime expiresAt);
        Task<bool> RevokeRefreshTokenAsync(string refreshTokenId);
        Task<(bool IsValid, string? UserId, string? tokenFamilyId, bool tokenReused)> ValidateRefreshTokenAsync(string refreshToken);
    }
}
