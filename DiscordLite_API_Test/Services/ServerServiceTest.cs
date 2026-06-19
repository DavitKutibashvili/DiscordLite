using DiscordLite_API.Data;
using DiscordLite_API.Model;
using DiscordLite_API.Services;
using Microsoft.EntityFrameworkCore;

namespace DiscordLite_API_Test.Services
{
    public class ServerServiceTest : IDisposable
    {
        private readonly ApplicationDbContext _db;
        private readonly ServerService _service;

        // Shared test user IDs
        private const string OwnerId = "owner-123";
        private const string MemberId = "member-456";
        private const string StrangerId = "stranger-789";

        public ServerServiceTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // fresh DB per test
                .Options;

            _db = new ApplicationDbContext(options);
            _service = new ServerService(_db);
        }

        public void Dispose() => _db.Dispose();

        // ── Helpers ────────────────────────────────────────────────────────────

        private async Task<Server> SeedServerAsync(string ownerId = OwnerId, string? inviteCode = null, DateTime? inviteExpiry = null)
        {
            var server = new Server
            {
                Name = "Test Server",
                OwnerId = ownerId,
                CreatedAt = DateTime.UtcNow,
                InviteCode = inviteCode,
                InviteExpiresAt = inviteExpiry
            };
            _db.Servers.Add(server);

            _db.ServerMembers.Add(new ServerMember
            {
                UserId = ownerId,
                Server = server,
                JoinedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return server;
        }

        private async Task AddMemberAsync(int serverId, string userId)
        {
            _db.ServerMembers.Add(new ServerMember
            {
                ServerId = serverId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        // ── CreateServerAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task CreateServer_ValidInput_ReturnsCreated()
        {
            var result = await _service.CreateServerAsync("My Server", OwnerId);

            Assert.Equal(201, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Equal("My Server", result.Data.Name);
            Assert.Equal(OwnerId, result.Data.OwnerId);
        }

        [Fact]
        public async Task CreateServer_EmptyName_ReturnsBadRequest()
        {
            var result = await _service.CreateServerAsync("", OwnerId);

            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task CreateServer_NameTooLong_ReturnsBadRequest()
        {
            var result = await _service.CreateServerAsync(new string('A', 51), OwnerId);

            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task CreateServer_EmptyUserId_ReturnsBadRequest()
        {
            var result = await _service.CreateServerAsync("Valid Name", "");

            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task CreateServer_PersistsServerMemberAndChannel()
        {
            await _service.CreateServerAsync("Test", OwnerId);

            Assert.True(await _db.ServerMembers.AnyAsync(m => m.UserId == OwnerId));
            Assert.True(await _db.ServerChannels.AnyAsync(c => c.Name == "General"));
        }

        // ── DeleteServerAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task DeleteServer_ByOwner_ReturnsOk()
        {
            var server = await SeedServerAsync();

            var result = await _service.DeleteServerAsync(server.Id, OwnerId);

            Assert.Equal(200, result.StatusCode);
            Assert.Null(await _db.Servers.FindAsync(server.Id));
        }

        [Fact]
        public async Task DeleteServer_ByNonOwner_ReturnsForbidden()
        {
            var server = await SeedServerAsync();

            var result = await _service.DeleteServerAsync(server.Id, MemberId);

            Assert.Equal(403, result.StatusCode);
        }

        [Fact]
        public async Task DeleteServer_InvalidId_ReturnsBadRequest()
        {
            var result = await _service.DeleteServerAsync(0, OwnerId);

            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task DeleteServer_NotFound_ReturnsNotFound()
        {
            var result = await _service.DeleteServerAsync(9999, OwnerId);

            Assert.Equal(404, result.StatusCode);
        }

        // ── GenerateInviteCodeAsync ─────────────────────────────────────────────

        [Fact]
        public async Task GenerateInviteCode_ByOwner_ReturnsCode()
        {
            var server = await SeedServerAsync();

            var result = await _service.GenerateInviteCodeAsync(server.Id, OwnerId);

            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Equal(8, result.Data.Length);
        }

        [Fact]
        public async Task GenerateInviteCode_ByNonOwner_ReturnsForbidden()
        {
            var server = await SeedServerAsync();

            var result = await _service.GenerateInviteCodeAsync(server.Id, MemberId);

            Assert.Equal(403, result.StatusCode);
        }

        [Fact]
        public async Task GenerateInviteCode_SetsExpiry7DaysFromNow()
        {
            var server = await SeedServerAsync();
            await _service.GenerateInviteCodeAsync(server.Id, OwnerId);

            var updated = await _db.Servers.FindAsync(server.Id);
            Assert.NotNull(updated!.InviteExpiresAt);
            Assert.True(updated.InviteExpiresAt > DateTime.UtcNow.AddDays(6));
        }

        // ── GetServerByIdAsync ──────────────────────────────────────────────────

        [Fact]
        public async Task GetServerById_AsMember_ReturnsOk()
        {
            var server = await SeedServerAsync();

            var result = await _service.GetServerByIdAsync(server.Id, OwnerId);

            Assert.Equal(200, result.StatusCode);
            Assert.Equal(server.Id, result.Data!.Id);
        }

        [Fact]
        public async Task GetServerById_AsNonMember_ReturnsForbidden()
        {
            var server = await SeedServerAsync();

            var result = await _service.GetServerByIdAsync(server.Id, StrangerId);

            Assert.Equal(403, result.StatusCode);
        }

        [Fact]
        public async Task GetServerById_NotFound_ReturnsNotFound()
        {
            var result = await _service.GetServerByIdAsync(9999, OwnerId);

            Assert.Equal(404, result.StatusCode);
        }

        // ── GetUserServersAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task GetUserServers_ReturnsOnlyUsersServers()
        {
            await SeedServerAsync(OwnerId);
            await SeedServerAsync(MemberId); // different owner, owner not a member

            var result = await _service.GetUserServersAsync(OwnerId);

            Assert.Equal(200, result.StatusCode);
            Assert.All(result.Data!, s => Assert.Equal(OwnerId, s.OwnerId));
        }

        [Fact]
        public async Task GetUserServers_IncludesServersJoinedAsMember()
        {
            var server = await SeedServerAsync(MemberId);
            await AddMemberAsync(server.Id, OwnerId);

            var result = await _service.GetUserServersAsync(OwnerId);

            Assert.Contains(result.Data!, s => s.Id == server.Id);
        }

        // ── JoinServerAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task JoinServer_ValidCode_ReturnsOk()
        {
            var server = await SeedServerAsync(inviteCode: "ABCD1234", inviteExpiry: DateTime.UtcNow.AddDays(7));

            var result = await _service.JoinServerAsync(StrangerId, "ABCD1234");

            Assert.Equal(200, result.StatusCode);
            Assert.True(await _db.ServerMembers.AnyAsync(m => m.ServerId == server.Id && m.UserId == StrangerId));
        }

        [Fact]
        public async Task JoinServer_InvalidCode_ReturnsNotFound()
        {
            var result = await _service.JoinServerAsync(StrangerId, "INVALID0");

            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task JoinServer_ExpiredCode_ReturnsBadRequest()
        {
            await SeedServerAsync(inviteCode: "EXPIRED1", inviteExpiry: DateTime.UtcNow.AddDays(-1));

            var result = await _service.JoinServerAsync(StrangerId, "EXPIRED1");

            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task JoinServer_AlreadyMember_ReturnsBadRequest()
        {
            var server = await SeedServerAsync(inviteCode: "ABCD1234", inviteExpiry: DateTime.UtcNow.AddDays(7));

            var result = await _service.JoinServerAsync(OwnerId, "ABCD1234"); // owner is already a member

            Assert.Equal(400, result.StatusCode);
        }

        // ── LeaveServerAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task LeaveServer_AsMember_ReturnsOk()
        {
            var server = await SeedServerAsync();
            await AddMemberAsync(server.Id, MemberId);

            var result = await _service.LeaveServerAsync(MemberId, server.Id);

            Assert.Equal(200, result.StatusCode);
            Assert.False(await _db.ServerMembers.AnyAsync(m => m.ServerId == server.Id && m.UserId == MemberId));
        }

        [Fact]
        public async Task LeaveServer_AsOwner_ReturnsBadRequest()
        {
            var server = await SeedServerAsync();

            var result = await _service.LeaveServerAsync(OwnerId, server.Id);

            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task LeaveServer_NotMember_ReturnsBadRequest()
        {
            var server = await SeedServerAsync();

            var result = await _service.LeaveServerAsync(StrangerId, server.Id);

            Assert.Equal(400, result.StatusCode);
        }

        // ── RemoveMemberAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task RemoveMember_ByOwner_ReturnsOk()
        {
            var server = await SeedServerAsync();
            await AddMemberAsync(server.Id, MemberId);

            var result = await _service.RemoveMemberAsync(OwnerId, MemberId, server.Id);

            Assert.Equal(200, result.StatusCode);
            Assert.False(await _db.ServerMembers.AnyAsync(m => m.ServerId == server.Id && m.UserId == MemberId));
        }

        [Fact]
        public async Task RemoveMember_ByNonOwner_ReturnsForbidden()
        {
            var server = await SeedServerAsync();
            await AddMemberAsync(server.Id, MemberId);

            var result = await _service.RemoveMemberAsync(StrangerId, MemberId, server.Id);

            Assert.Equal(403, result.StatusCode);
        }

        [Fact]
        public async Task RemoveMember_RemoveSelf_ReturnsBadRequest()
        {
            var server = await SeedServerAsync();

            var result = await _service.RemoveMemberAsync(OwnerId, OwnerId, server.Id);

            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task RemoveMember_TargetNotInServer_ReturnsBadRequest()
        {
            var server = await SeedServerAsync();

            var result = await _service.RemoveMemberAsync(OwnerId, StrangerId, server.Id);

            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task RemoveMember_EmptyMemberId_ReturnsBadRequest()
        {
            var server = await SeedServerAsync();

            var result = await _service.RemoveMemberAsync(OwnerId, "", server.Id);

            Assert.Equal(400, result.StatusCode);
        }
    }
}