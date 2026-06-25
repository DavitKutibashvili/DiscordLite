using DiscordLite_Utility;
using DiscordLite_WEB.Models;
using DiscordLite_WEB.Services.IServices;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Channels;

namespace DiscordLite_WEB.Services
{
    public class DMChatService : BaseService, IDMChatService
    {
        private readonly string ApiEndpoint = "/api/DMChat";
        public DMChatService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider, IHttpContextAccessor httpContextAccessor) : base(httpClientFactory, tokenProvider, httpContextAccessor)
        {
        }

        public Task<T> GetChatAsync<T>(int chatId)
        {
            return SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = ApiEndpoint + "/" + chatId
            })!;
        }

        public Task<T> GetChatMessagesAsync<T>(int chatId, int page, int pageSize)
        {
            return SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{ApiEndpoint}/{chatId}/messages?page={page}&pageSize={pageSize}"
            })!;
        }

        public Task<T> GetChatsAsync<T>()
        {
            return SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = ApiEndpoint
            })!;
        }
    }
}
