using System.ComponentModel.DataAnnotations;
using ChatApp.Model.Models;

namespace ChatApp.Model.Models;

public class ChatRoomMembership
{
    [Key] public int Id { get; set; }

    [Required] public string ChatRoomId { get; set; } = default!;
    public ChatRoom ChatRoom { get; set; } = default!;

    // SignalR connection for this session (always present)
    [Required] public string ConnectionId { get; set; } = default!;

    // ───── Identity – optional ─────
    public string?          UserId { get; set; }          // nullable ⇒ guests allowed
    public ApplicationUser? User   { get; set; }

    // ───── Guests – optional ─────
    public string? Nickname { get; set; }                 // store guest display name

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}