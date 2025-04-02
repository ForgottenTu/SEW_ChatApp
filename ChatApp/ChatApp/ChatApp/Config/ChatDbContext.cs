using ChatApp.Model;

namespace ChatApp.Config;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
public class ChatDbContext : DbContext
{ 
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
    {
        
    }
    public DbSet<ChatRoom> ChatRooms { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ChatMessage> Messages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=chatapp.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatRoom>()
            .HasMany(c => c.Users)
            .WithOne(u => u.ChatRoom)
            .HasForeignKey(u => u.ChatRoomId);

        modelBuilder.Entity<ChatMessage>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.Messages)
            .HasForeignKey(m => m.SenderId);

        modelBuilder.Entity<ChatMessage>()
            .HasOne(m => m.ChatRoom)
            .WithMany(r => r.Messages)
            .HasForeignKey(m => m.RoomId);
    }
}


