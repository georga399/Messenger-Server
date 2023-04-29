using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Messenger.Models;

namespace Messenger.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public DbSet<Chat> Chats { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<Connection> Connections { get; set; } = null!;
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<ChatUser>()
        .HasKey(t => new { t.ChatId, t.UserId });

        modelBuilder.Entity<Chat>()
        .HasMany(c => c.ChatUsers)
        .WithOne(cu => cu.Chat);

        modelBuilder.Entity<Chat>()
        .HasOne(c=>c.Admin)
        .WithMany(u=>u.AdministrateChats);

        modelBuilder.Entity<User>().HasIndex(u => u.IntId).IsUnique();
    }
}