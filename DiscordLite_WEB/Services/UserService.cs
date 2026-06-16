using DiscordLite_Utility;
using DiscordLite_WEB.Extensions;
using DiscordLite_WEB.Models;
using DiscordLite_WEB.Services.IServices;
using System.Net.Http.Headers;

namespace DiscordLite_WEB.Services
{
    public class UserService : BaseService, IUserService
    {
        public UserService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider, IHttpContextAccessor httpContextAccessor) : base(httpClientFactory, tokenProvider, httpContextAccessor)
        {
        }

        public Task<T?> DeleteUserAvatarAsync<T>()
        {
            return SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.DELETE,
                Url = "/api/user/avatar"
            });
        }

        public Task<T?> GetUserProfileAsync<T>()
        {
            return SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = "/api/user/me"
            });
        }

        public Task<T?> UpdateDisplayNameAsync<T>(string newDisplayName)
        {
            return SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.PATCH,
                Url = $"/api/user/displayname?newDisplayName={Uri.EscapeDataString(newDisplayName)}",
            });
        }

        public Task<T?> UpdateUserAvatarAsync<T>(IFormFile formFile)
        {
            var formData = new MultipartFormDataContent();
            var streamContent = new StreamContent(formFile.OpenReadStream());
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(formFile.ContentType);
            formData.Add(streamContent, "file", formFile.FileName);
            return SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.PATCH,
                Url = "/api/user/avatar",
                Data = formData
            });
        }
    }
}
