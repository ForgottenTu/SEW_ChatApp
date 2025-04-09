using ChatApp.Model.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Model.Context;

public class ChatAppContext: IdentityDbContext<ApplicationUser>
{
    public ChatAppContext(DbContextOptions<ChatAppContext> options)
        : base(options)
    {}
    public DbSet<Chatroom> Chatrooms { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Chatroom)
            .WithMany(c => c.Users)
            .HasForeignKey("ChatroomId") 
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.Entity<Message>()
            .HasOne(m => m.Chatroom)
            .WithMany()
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Entity<Message>()
            .HasOne<ApplicationUser>()  
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);
 



    }
}