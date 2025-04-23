using System.ComponentModel.DataAnnotations;

namespace ChatApp.Model.Models;

public class ChatMessage
{
    [Key] public long Id { get; set; }

    [Required] public string ChatRoomId { get; set; } = default!;
    public ChatRoom ChatRoom { get; set; } = default!;

    public string? UserId { get; set; }          // nullable for guests
    public ApplicationUser? User { get; set; }

    [Required] public string Sender   { get; set; } = default!; // falls back to nickname
    [Required] public string Text     { get; set; } = default!;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}