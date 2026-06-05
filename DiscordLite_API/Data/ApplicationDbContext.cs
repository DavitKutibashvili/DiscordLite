using DiscordLite_API.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DiscordLite_API.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<DirectMessageChat> DirectMessageChats { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // ── Friendship ─────────────────-
            modelBuilder.Entity<Friendship>(e =>
            {
                // Prevents duplicate friendship rows in either direction
                e.HasIndex(f => new { f.RequestedById, f.ReceivedById }).IsUnique();

                e.HasOne(f => f.RequestedBy)
                    .WithMany()
                    .HasForeignKey(f => f.RequestedById)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(f => f.ReceivedBy)
                    .WithMany()
                    .HasForeignKey(f => f.ReceivedById)
                    .OnDelete(DeleteBehavior.Restrict);

                // Stores "Pending", "Accepted" etc. instead of 0, 1 — more readable in DB
                e.Property(f => f.Status)
                    .HasConversion<string>();
            });

            // ── DirectMessageChat ────────────-
            modelBuilder.Entity<DirectMessageChat>(e =>
            {
                // Only one DM chat can exist between any two users
                e.HasIndex(c => new { c.User1Id, c.User2Id }).IsUnique();

                e.HasOne(c => c.User1)
                    .WithMany()
                    .HasForeignKey(c => c.User1Id)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(c => c.User2)
                    .WithMany()
                    .HasForeignKey(c => c.User2Id)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Message ─────────────────────
            modelBuilder.Entity<Message>(e =>
            {
                e.HasOne(m => m.Chat)
                    .WithMany(c => c.Messages)
                    .HasForeignKey(m => m.ChatId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(m => m.Sender)
                    .WithMany()
                    .HasForeignKey(m => m.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
