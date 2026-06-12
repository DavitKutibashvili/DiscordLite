using DiscordLite_API.Services.IServices;
using System.Collections.Concurrent;

namespace DiscordLite_API.Services
{
    public class PresenceService : IPresenceService
    {
        private readonly ConcurrentDictionary<string, int> _onlineUsers = new();

        public void UserConnected(string userId)
        {
            _onlineUsers.AddOrUpdate(userId, 1, (_, count) => count + 1);
        }

        /// <returns>True if this was the user's last connection (should broadcast UserOffline)</returns>
        public bool UserDisconnected(string userId)
        {
            if (!_onlineUsers.TryGetValue(userId, out var count))
                return false;

            if (count <= 1)
            {
                _onlineUsers.TryRemove(userId, out _);
                return true;
            }

            _onlineUsers.TryUpdate(userId, count - 1, count);
            return false;
        }

        public bool IsOnline(string userId) => _onlineUsers.ContainsKey(userId);

        public IReadOnlyList<string> GetOnlineUsers() => _onlineUsers.Keys.ToList();

        public IReadOnlyList<string> GetOnlineUsers(IEnumerable<string> userIds)
        {
            var userIdSet = new HashSet<string>(userIds);
            return _onlineUsers.Keys.Where(userIdSet.Contains).ToList();
        }
    }
}
