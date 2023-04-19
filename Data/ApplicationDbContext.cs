using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Messenger.Models;

namespace Messenger.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public DbSet<Chat> Chats { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<Connection> Connections {get; set;} = null!;
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder
            .Entity<Chat>()
            .HasMany(c => c.Users)
            .WithMany(u => u.Chats)
            .UsingEntity<ChatUser>(
               j => j
                .HasOne(pt => pt.User)
                .WithMany(t => t.ChatUsers)
                .HasForeignKey(pt => pt.UserId),
            j => j
                .HasOne(pt => pt.Chat)
                .WithMany(p => p.ChatUsers)
                .HasForeignKey(pt => pt.ChatId),
            j =>
            {
                j.HasKey(t => new { t.ChatId, t.UserId });
                j.ToTable("ChatUser");
            });
            modelBuilder.Entity<User>().HasIndex(u => u.IntId).IsUnique();
    }
}