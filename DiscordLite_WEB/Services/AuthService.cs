using DiscordLite_DTO;
using DiscordLite_Utility;
using DiscordLite_WEB.Models;
using DiscordLite_WEB.Services.IServices;

namespace DiscordLite_WEB.Services
{
    public class AuthService : BaseService, IAuthService
    {
        private const string APIEndpoint = "/api/auth";
        public AuthService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, ITokenProvider tokenProvider) : base(httpClientFactory, tokenProvider, httpContextAccessor)
        {

        }
        public Task<T?> LoginAsync<T>(LoginRequestDTO loginRequestDTO)
        {
            return SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = loginRequestDTO,
                Url = APIEndpoint + "/login"
            }, withBearer: false);
        }

        public Task<T?> RefreshTokenAsync<T>(RefreshTokenRequestDTO refreshTokenRequestDTO)
        {
            return SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = refreshTokenRequestDTO,
                Url = APIEndpoint + "/refresh-token"
            }, withBearer: false);
        }

        public Task<T?> RegisterAsync<T>(RegistrationRequestDTO registrationRequestDTO)
        {
            return SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = registrationRequestDTO,
                Url = APIEndpoint + "/register"
            }, withBearer: false);
        }
    }
}
