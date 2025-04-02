namespace ChatApp.Model;

public class ChatMessage
{
    public int Id { get; set; }
    public string Content { get; set; }
    public string SenderId { get; set; }
    public User Sender { get; set; }
    public string RoomId { get; set; }
    public ChatRoom ChatRoom { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
