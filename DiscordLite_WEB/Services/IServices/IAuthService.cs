using DiscordLite_DTO;

namespace DiscordLite_WEB.Services.IServices
{
    public interface IAuthService
    {
        Task<T?> LoginAsync<T>(LoginRequestDTO loginRequestDTO);
        Task<T?> RegisterAsync<T>(RegistrationRequestDTO registrationRequestDTO);
        Task<T?> RefreshTokenAsync<T>(RefreshTokenRequestDTO refreshTokenRequestDTO);
    }
}
