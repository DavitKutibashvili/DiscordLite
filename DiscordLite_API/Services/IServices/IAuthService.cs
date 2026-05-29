using DiscordLite_DTO;

namespace DiscordLite_API.Services.IServices
{
    public interface IAuthService
    {
        Task<(UserDTO?, string? error)> RegisterAsync(RegistrationRequestDTO registrationRequestDTO);
        Task<TokenDTO?> LoginAsync(LoginRequestDTO loginRequestDTO);
        Task<TokenDTO?> RefreshAccessTokenAsync(RefreshTokenRequestDTO refreshTokenRequestDTO);
    }
}
