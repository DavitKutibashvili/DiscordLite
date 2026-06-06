using DiscordLite_DTO;
using DiscordLite_Utility;
using DiscordLite_WEB.Models;
using DiscordLite_WEB.Services.IServices;

namespace DiscordLite_WEB.Services
{
    public class FriendService : BaseService, IFriendService
    {
        private const string APIEndpoint = "/api/Friendships";
        public FriendService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider, IHttpContextAccessor httpContextAccessor) : base(httpClientFactory, tokenProvider, httpContextAccessor)
        {
            
        }

        public Task<T> AcceptFriendRequestAsync<T>(int requestId)
        {
            return SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.PUT,
                Url = APIEndpoint  + $"/{requestId}/accept"
            })!;
        }

        public Task<T> DeclineFriendRequestAsync<T>(int requestId)
        {
            return SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.PUT,
                Url = APIEndpoint + $"/{requestId}/decline"
            })!;
        }

        public Task<T> GetFriendRequestsAsync<T>()
        {
            return SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = APIEndpoint + "/pending"
            })!;
        }

        public Task<T> GetFriendsAsync<T>()
        {
            return SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = APIEndpoint
            })!;
        }

        public Task<T> RemoveFriendAsync<T>(int friendshipId)
        {
            return SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.DELETE,
                Url = APIEndpoint + $"/{friendshipId}/remove",
            })!;
        }

        public Task<T> SendFriendRequestAsync<T>(string username)
        {
            return SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.POST,
                Url = APIEndpoint + $"/request/{username}"
            })!;
        }
    }
}
