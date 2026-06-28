using DiscordLite_DTO;
using DiscordLite_Utility;
using DiscordLite_WEB.Models;
using DiscordLite_WEB.Services.IServices;

namespace DiscordLite_WEB.Services
{
    public class ServerService : BaseService, IServerService
    {
        public ServerService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider, IHttpContextAccessor httpContextAccessor) : base(httpClientFactory, tokenProvider, httpContextAccessor)
        {
        }

        private const string baseUrl = "/api/Server/";

        public Task<T> CreateServerAsync<T>(string name)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Url = baseUrl + $"create?name={name}"
            })!;
        }

        public Task<T> DeleteServerAsync<T>(int serverId)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.DELETE,
                Url = baseUrl + serverId
            })!;
        }

        public Task<T> GenerateInviteCodeAsync<T>(int serverId)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.PATCH,
                Url = baseUrl + $"generate-invite/{serverId}"
            })!;
        }

        public Task<T> GetServerByIdAsync<T>(int serverId)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = baseUrl + serverId
            })!;
        }

        public Task<T> GetUserServersAsync<T>()
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = baseUrl + "user-servers"
            })!;
        }

        public Task<T> JoinServerAsync<T>(string inviteCode)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Url = baseUrl + $"join/{inviteCode}"
            })!;
        }

        public Task<T> LeaveServerAsync<T>(int serverId)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.DELETE,
                Url = baseUrl + $"leave/{serverId}"
            })!;
        }

        public Task<T> RemoveMemberAsync<T>(int serverId, string memberId)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.DELETE,
                Url = baseUrl + $"{serverId}/{memberId}"
            })!;
        }
        public Task<T> GetChannelMessagesAsync<T>(int channelId, int page, int pageSize)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = $"api/channel/messages?channelId={channelId}&page={page}&pageSize={pageSize}"
            })!;
        }
        public Task<T> CreateChannelAsync<T>(ChannelCreateDTO dto)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Url = $"api/channel",
                Data = dto
            })!;
        }
        public Task<T> DeleteChannelAsync<T>(int id)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.DELETE,
                Url = $"api/Channel?channelId={id}",
            })!;
        }
    }
}
