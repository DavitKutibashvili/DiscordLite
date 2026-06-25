using DiscordLite_API.Data;
using DiscordLite_API.Model;
using DiscordLite_API.Services.IServices;
using DiscordLite_DTO;
using DiscordLite_Utility;
using Microsoft.EntityFrameworkCore;

namespace DiscordLite_API.Services
{
    public class ServerService : IServerService
    {
        private readonly ApplicationDbContext _db;
        public ServerService(ApplicationDbContext applicationDbContext)
        {
            _db = applicationDbContext;
        }

        public async Task<ApiResponse<ServerDTO>> CreateServerAsync(string name, string userId)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length > 50)
            {
                return ApiResponse<ServerDTO>.BadRequest("Server name must be between 1 and 50 characters long.");
            }
            if (string.IsNullOrWhiteSpace(userId))
            {
                return ApiResponse<ServerDTO>.BadRequest("User ID is required.");
            }
            var server = new Server
            {
                Name = name,
                OwnerId = userId,
                CreatedAt = DateTime.UtcNow
            };
            var serverMember = new ServerMember
            {
                UserId = userId,
                Server = server,
                JoinedAt = DateTime.UtcNow
            };
            var serverChannel = new ServerChannel
            {
                Name = "General",
                Server = server,
                Position = 0,
                Type = SD.ChannelType.Text
            };
            await _db.ServerMembers.AddAsync(serverMember);
            await _db.ServerChannels.AddAsync(serverChannel);
            await _db.Servers.AddAsync(server);
            await _db.SaveChangesAsync();
            return ApiResponse<ServerDTO>.CreatedAt(MapToDTO(server), "Server created successfully.");
        }

        public async Task<ApiResponse<object>> DeleteServerAsync(int serverId, string userId)
        {
            if(serverId <= 0)
            {
                return ApiResponse<object>.BadRequest("Invalid server ID.");
            }
            var server = await _db.Servers.FindAsync(serverId);
            if(server == null)
            {
                return ApiResponse<object>.NotFound("Server not found.");
            }
            bool isOwner = server.OwnerId == userId;
            if(!isOwner)
            {
                return ApiResponse<object>.Forbidden("User is not owner of the server");
            }
            _db.Servers.Remove(server);
            await _db.SaveChangesAsync();
            return ApiResponse<object>.Ok(null!, "Server deleted successfully.");
        }

        public async Task<ApiResponse<string>> GenerateInviteCodeAsync(int serverId, string userId)
        {
            if (serverId <= 0)
            {
                return ApiResponse<string>.BadRequest("Invalid server ID.");
            }
            var server = await _db.Servers.FindAsync(serverId);
            if (server == null)
            {
                return ApiResponse<string>.NotFound("Server not found.");
            }
            bool isOwner = server.OwnerId == userId;
            if(!isOwner)
            {
                return ApiResponse<string>.Forbidden("User is not owner of the server");
            }
            string newInviteCode = GenerateInviteCode();
            server.InviteCode = newInviteCode;
            server.InviteExpiresAt = DateTime.UtcNow.AddDays(7);
            await _db.SaveChangesAsync();
            return ApiResponse<string>.Ok(newInviteCode, "Invite code generated successfully.");
        }

        public async Task<ApiResponse<ServerDTO>> GetServerByIdAsync(int serverId, string userId)
        {
            if (serverId <= 0)
            {
                return ApiResponse<ServerDTO>.BadRequest("Invalid server ID.");
            }
            var isMember = await _db.ServerMembers.AnyAsync(sm => sm.ServerId == serverId && sm.UserId == userId);
            if (!isMember)
            {
                return ApiResponse<ServerDTO>.Forbidden("You are not a member of this server.");
            }
            var server = await _db.Servers
                .Include(s => s.Members)
                    .ThenInclude(m => m.User)
                .Include(s => s.Channels)
                .FirstOrDefaultAsync(s => s.Id == serverId);
            if (server == null)
            {
                return ApiResponse<ServerDTO>.NotFound("Server not found.");
            }
            return ApiResponse<ServerDTO>.Ok(MapToDTO(server), "Server retrieved successfully.");
        }

        public async Task<ApiResponse<List<ServerDTO>>> GetUserServersAsync(string userId)
        {
            if(userId == null)
            {
                return ApiResponse<List<ServerDTO>>.BadRequest("User ID is required.");
            }
            var serversList = await _db.Servers.Where(s => s.Members.Any(m => m.UserId == userId)).ToListAsync();

            var serverDTOs = serversList.Select(MapToDTO).ToList();
            return ApiResponse<List<ServerDTO>>.Ok(serverDTOs, "User servers retrieved successfully.");
        }

        public async Task<ApiResponse<object>> JoinServerAsync(string userId, string inviteCode)
        {
            var server = await _db.Servers.FirstOrDefaultAsync(s => s.InviteCode == inviteCode);
            if(server == null)
            {
                return ApiResponse<object>.NotFound("Invite code does not exist.");
            }
            if(server.InviteExpiresAt < DateTime.UtcNow)
            {
                return ApiResponse<object>.BadRequest("Invite code has expired.");
            }
            var alreadyMember = await _db.ServerMembers.AnyAsync(m => m.ServerId == server.Id && m.UserId == userId);
            if (alreadyMember)
            {
                return ApiResponse<object>.BadRequest("You are already a member of this server.");
            }
            await _db.ServerMembers.AddAsync(new ServerMember
            {
                UserId = userId,
                ServerId = server.Id,
                JoinedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
            return ApiResponse<object>.Ok(null!, "Successfully joined the server.");
        }

        public async Task<ApiResponse<object>> LeaveServerAsync(string userId, int serverId)
        {
            if (serverId <= 0)
            {
                return ApiResponse<object>.BadRequest("Invalid server ID.");
            }
            var server = await _db.Servers.FindAsync(serverId);
            if (server == null)
            {
                return ApiResponse<object>.NotFound("Server not found.");
            }
            if (server.OwnerId == userId)
            {
                return ApiResponse<object>.BadRequest("You are the owner of this server. Delete the server instead.");
            }
            var serverMember = await _db.ServerMembers.FirstOrDefaultAsync(m => m.ServerId == serverId && m.UserId == userId);
            if(serverMember == null)
            {
                return ApiResponse<object>.BadRequest("You are not a member of this server.");
            }
            _db.ServerMembers.Remove(serverMember);
            await _db.SaveChangesAsync();
            return ApiResponse<object>.Ok(null!, "Successfully left the server.");
        }

        public async Task<ApiResponse<object>> RemoveMemberAsync(string ownerId, string memberId, int serverId)
        {
            if (serverId <= 0)
            {
                return ApiResponse<object>.BadRequest("Invalid server ID.");
            }
            var server = await _db.Servers.FindAsync(serverId);
            if (server == null)
            {
                return ApiResponse<object>.NotFound("Server not found.");
            }
            if (server.OwnerId != ownerId)
            {
                return ApiResponse<object>.Forbidden("You are not the owner of this server.");
            }
            if (string.IsNullOrEmpty(memberId))
            {
                return ApiResponse<object>.BadRequest("Member ID is required.");
            }
            if (ownerId == memberId)
            {
                return ApiResponse<object>.BadRequest("You cannot remove yourself from the server. Delete the server instead.");
            }
            var serverMember = await _db.ServerMembers.FirstOrDefaultAsync(m => m.ServerId == serverId && m.UserId == memberId);
            if (serverMember == null)
            {
                return ApiResponse<object>.BadRequest("Target user is not a member of this server.");
            }
            _db.ServerMembers.Remove(serverMember);
            await _db.SaveChangesAsync();
            return ApiResponse<object>.Ok(null!, "Successfully removed a member from the server.");
        }

        private static string GenerateInviteCode()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Range(0, 8)
                .Select(_ => chars[Random.Shared.Next(chars.Length)])
                .ToArray());
        }
        private ServerDTO MapToDTO(Server s) => new ServerDTO
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            IconUrl = s.IconUrl,
            OwnerId = s.OwnerId,
            CreatedAt = s.CreatedAt,
            InviteCode = s.InviteCode,
            Channels = s.Channels?.Select(c => new ServerChannelDTO
            {
                Id = c.Id,
                Name = c.Name
            }).ToList() ?? new(),
            Members = s.Members?.Select(m => new ServerMemberDTO
            {
                UserId = m.UserId,
                UserName = m.User.UserName!,
                DisplayName = m.User.DisplayName,
                AvatarUrl = m.User.AvatarUrl,
                IsOwner = m.UserId == s.OwnerId
            }).ToList() ?? new()
        };
    }
}
