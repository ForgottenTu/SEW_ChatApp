using ChatApp.Model.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Model.Context;

public class ChatAppContext
    : IdentityDbContext<ApplicationUser>
{
    public ChatAppContext(DbContextOptions<ChatAppContext> options) : base(options)
    {
    }

    public DbSet<ChatRoom> ChatRooms { get; set; }
    public DbSet<ChatRoomMembership> ChatRoomMembers { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<ChatRoom>()
            .HasIndex(r => r.Name)
            .IsUnique();

        b.Entity<ChatRoomMembership>()
            .HasOne(m => m.ChatRoom)
            .WithMany(r => r.Members)
            .HasForeignKey(m => m.ChatRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<ChatMessage>()
            .HasOne(m => m.ChatRoom)
            .WithMany(r => r.Messages)
            .HasForeignKey(m => m.ChatRoomId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}