using System.ComponentModel.DataAnnotations;

namespace ChatApp.Model.Models;

public class ChatRoom
{
    [Key] public string Id { get; set; } = Guid.NewGuid().ToString();
    [Required, MaxLength(100)] public string Name { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ChatRoomMembership> Members { get; set; } = new List<ChatRoomMembership>();
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}