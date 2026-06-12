namespace DiscordLite_API.Services.IServices
{
    public interface IPresenceService
    {
        void UserConnected(string userId);
        bool UserDisconnected(string userId);
        bool IsOnline(string userId);
        public IReadOnlyList<string> GetOnlineUsers();
        IReadOnlyList<string> GetOnlineUsers(IEnumerable<string> userIds);
    }
}
