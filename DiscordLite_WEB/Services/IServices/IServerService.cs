namespace DiscordLite_WEB.Services.IServices
{
    public interface IServerService
    {
        Task<T> GetUserServersAsync<T>();
        Task<T> CreateServerAsync<T>(string name);
        Task<T> GetServerByIdAsync<T>(int serverId);
        Task<T> DeleteServerAsync<T>(int serverId);
        Task<T> GenerateInviteCodeAsync<T>(int serverId);
        Task<T> JoinServerAsync<T>(string inviteCode);
        Task<T> LeaveServerAsync<T>(int serverId);
        Task<T> RemoveMemberAsync<T>(int serverId, string memberId);
        Task<T> GetChannelMessagesAsync<T>(int channelId, int page, int pageSize);
    }
}
