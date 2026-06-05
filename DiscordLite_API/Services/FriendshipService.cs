using DiscordLite_API.Data;
using DiscordLite_API.Model;
using DiscordLite_API.Services.IServices;
using DiscordLite_DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DiscordLite_API.Services
{
    public class FriendshipService : IFriendshipService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;

        public FriendshipService(ApplicationDbContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // ── Send Friend Request ──────────────────────────────────────────────────

        public async Task<ApiResponse<FriendshipDTO>> SendFriendRequestAsync(string requestedById, string receivedByUsername)
        {
            // Resolve username to user
            var receivedBy = await _userManager.FindByNameAsync(receivedByUsername);
            if (receivedBy == null)
                return ApiResponse<FriendshipDTO>.NotFound("User not found.");

            // Can't send a request to yourself
            if (requestedById == receivedBy.Id)
                return ApiResponse<FriendshipDTO>.BadRequest("You cannot send a friend request to yourself.");

            // Check if any friendship record already exists in either direction
            var existing = await _db.Friendships.FirstOrDefaultAsync(f =>
                (f.RequestedById == requestedById && f.ReceivedById == receivedBy.Id) ||
                (f.RequestedById == receivedBy.Id && f.ReceivedById == requestedById));

            if (existing != null)
            {
                return existing.Status switch
                {
                    FriendshipStatus.Pending => ApiResponse<FriendshipDTO>.Conflict("Friend request already sent."),
                    FriendshipStatus.Accepted => ApiResponse<FriendshipDTO>.Conflict("You are already friends."),
                    FriendshipStatus.Blocked => ApiResponse<FriendshipDTO>.Conflict("Unable to send friend request."),

                    // Declined — allow re-sending by resetting the record
                    FriendshipStatus.Declined => await ResetFriendRequestAsync(existing, requestedById, receivedBy.Id),
                    _ => ApiResponse<FriendshipDTO>.BadRequest("Unexpected friendship state.")
                };
            }

            var friendship = new Friendship
            {
                RequestedById = requestedById,
                ReceivedById = receivedBy.Id,
                Status = FriendshipStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Friendships.Add(friendship);
            await _db.SaveChangesAsync();

            await _db.Entry(friendship).Reference(f => f.RequestedBy).LoadAsync();
            await _db.Entry(friendship).Reference(f => f.ReceivedBy).LoadAsync();

            return ApiResponse<FriendshipDTO>.Ok(MapToDto(friendship));
        }

        // ── Accept Friend Request ────────────────────────────────────────────────

        public async Task<ApiResponse<FriendshipDTO>> AcceptFriendRequestAsync(int friendshipId, string currentUserId)
        {
            var friendship = await _db.Friendships
                .Include(f => f.RequestedBy)
                .Include(f => f.ReceivedBy)
                .FirstOrDefaultAsync(f => f.Id == friendshipId);

            if (friendship == null)
                return ApiResponse<FriendshipDTO>.NotFound("Friend request not found.");

            // Only the person who received the request can accept it
            if (friendship.ReceivedById != currentUserId)
                return ApiResponse<FriendshipDTO>.Forbidden("You are not authorized to accept this request.");

            if (friendship.Status != FriendshipStatus.Pending)
                return ApiResponse<FriendshipDTO>.BadRequest("This request is no longer pending.");

            friendship.Status = FriendshipStatus.Accepted;
            friendship.UpdatedAt = DateTime.UtcNow;

            // Create DM chat atomically — both happen in one SaveChangesAsync
            var (user1Id, user2Id) = OrderUserIds(friendship.RequestedById, friendship.ReceivedById);
            var chatExists = await _db.DirectMessageChats
                .AnyAsync(c => c.User1Id == user1Id && c.User2Id == user2Id);

            if (!chatExists)
            {
                _db.DirectMessageChats.Add(new DirectMessageChat
                {
                    User1Id = user1Id,
                    User2Id = user2Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();
            return ApiResponse<FriendshipDTO>.Ok(MapToDto(friendship));
        }

        // ── Decline Friend Request ───────────────────────────────────────────────

        public async Task<ApiResponse<FriendshipDTO>> DeclineFriendRequestAsync(int friendshipId, string currentUserId)
        {
            var friendship = await _db.Friendships
                .Include(f => f.RequestedBy)
                .Include(f => f.ReceivedBy)
                .FirstOrDefaultAsync(f => f.Id == friendshipId);

            if (friendship == null)
                return ApiResponse<FriendshipDTO>.NotFound("Friend request not found.");

            if (friendship.ReceivedById != currentUserId)
                return ApiResponse<FriendshipDTO>.Forbidden("You are not authorized to decline this request.");

            if (friendship.Status != FriendshipStatus.Pending)
                return ApiResponse<FriendshipDTO>.BadRequest("This request is no longer pending.");

            friendship.Status = FriendshipStatus.Declined;
            friendship.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return ApiResponse<FriendshipDTO>.Ok(MapToDto(friendship));
        }

        // ── Remove Friend ────────────────────────────────────────────────────────

        public async Task<ApiResponse<FriendshipDTO>> RemoveFriendAsync(int friendshipId, string currentUserId)
        {
            var friendship = await _db.Friendships
                .Include(f => f.RequestedBy)
                .Include(f => f.ReceivedBy)
                .FirstOrDefaultAsync(f => f.Id == friendshipId);

            if (friendship == null)
                return ApiResponse<FriendshipDTO>.NotFound("Friendship not found.");

            // Either user in the friendship can remove it
            if (friendship.RequestedById != currentUserId && friendship.ReceivedById != currentUserId)
                return ApiResponse<FriendshipDTO>.Forbidden("You are not part of this friendship.");

            if (friendship.Status != FriendshipStatus.Accepted)
                return ApiResponse<FriendshipDTO>.BadRequest("You are not friends with this user.");

            _db.Friendships.Remove(friendship);
            await _db.SaveChangesAsync();

            return ApiResponse<FriendshipDTO>.Ok(MapToDto(friendship));
        }

        // ── Block User ───────────────────────────────────────────────────────────

        public async Task<ApiResponse<FriendshipDTO>> BlockUserAsync(int friendshipId, string currentUserId)
        {
            var friendship = await _db.Friendships
                .Include(f => f.RequestedBy)
                .Include(f => f.ReceivedBy)
                .FirstOrDefaultAsync(f => f.Id == friendshipId);

            if (friendship == null)
                return ApiResponse<FriendshipDTO>.NotFound("Friendship not found.");

            if (friendship.RequestedById != currentUserId && friendship.ReceivedById != currentUserId)
                return ApiResponse<FriendshipDTO>.Forbidden("You are not part of this friendship.");

            if (friendship.Status == FriendshipStatus.Blocked)
                return ApiResponse<FriendshipDTO>.BadRequest("User is already blocked.");

            friendship.Status = FriendshipStatus.Blocked;
            friendship.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return ApiResponse<FriendshipDTO>.Ok(MapToDto(friendship));
        }

        // ── Get Friends ──────────────────────────────────────────────────────────

        public async Task<ApiResponse<List<FriendshipDTO>>> GetFriendsAsync(string currentUserId)
        {
            var friends = await _db.Friendships
                .Include(f => f.RequestedBy)
                .Include(f => f.ReceivedBy)
                .Where(f =>
                    f.Status == FriendshipStatus.Accepted &&
                    (f.RequestedById == currentUserId || f.ReceivedById == currentUserId))
                .ToListAsync();

            return ApiResponse<List<FriendshipDTO>>.Ok(friends.Select(MapToDto).ToList());
        }

        // ── Get Pending Requests ─────────────────────────────────────────────────

        public async Task<ApiResponse<List<FriendshipDTO>>> GetPendingRequestsAsync(string currentUserId)
        {
            var pending = await _db.Friendships
                .Include(f => f.RequestedBy)
                .Include(f => f.ReceivedBy)
                .Where(f =>
                    f.Status == FriendshipStatus.Pending &&
                    f.ReceivedById == currentUserId)  // only requests sent TO you
                .ToListAsync();

            return ApiResponse<List<FriendshipDTO>>.Ok(pending.Select(MapToDto).ToList());
        }

        // ── Private Helpers ──────────────────────────────────────────────────────

        private async Task<ApiResponse<FriendshipDTO>> ResetFriendRequestAsync(
            Friendship existing, string newRequestedById, string newReceivedById)
        {
            // Flip direction if needed — the new requester might be the opposite person
            existing.RequestedById = newRequestedById;
            existing.ReceivedById = newReceivedById;
            existing.Status = FriendshipStatus.Pending;
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            await _db.Entry(existing).Reference(f => f.RequestedBy).LoadAsync();
            await _db.Entry(existing).Reference(f => f.ReceivedBy).LoadAsync();

            return ApiResponse<FriendshipDTO>.Ok(MapToDto(existing));
        }

        private static (string User1Id, string User2Id) OrderUserIds(string a, string b)
            => string.Compare(a, b, StringComparison.Ordinal) < 0 ? (a, b) : (b, a);

        private static FriendshipDTO MapToDto(Friendship f) => new()
        {
            Id = f.Id,
            RequestedById = f.RequestedById,
            RequestedByUsername = f.RequestedBy.UserName!,
            ReceivedById = f.ReceivedById,
            ReceivedByUsername = f.ReceivedBy.UserName!,
            Status = f.Status.ToString(),
            CreatedAt = f.CreatedAt,
            UpdatedAt = f.UpdatedAt
        };
    }
}
