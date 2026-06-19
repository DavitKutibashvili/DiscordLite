using DiscordLite_DTO;

namespace DiscordLite_API.Services.IServices
{
    public interface IServerService
    {
        Task<ApiResponse<ServerDTO>> CreateServerAsync(string name, string userId);
        Task<ApiResponse<List<ServerDTO>>> GetUserServersAsync(string userId);
        Task<ApiResponse<ServerDTO>> GetServerByIdAsync(int serverId, string userId);
        Task<ApiResponse<object>> DeleteServerAsync(int serverId, string userId);
        Task<ApiResponse<string>> GenerateInviteCodeAsync(int serverId, string userId);
        Task<ApiResponse<object>> JoinServerAsync(string userId,string inviteCode);
        Task<ApiResponse<object>> LeaveServerAsync(string userId, int serverId);
        Task<ApiResponse<object>> RemoveMemberAsync(string ownerId, string memberId, int serverId);
    }
}
