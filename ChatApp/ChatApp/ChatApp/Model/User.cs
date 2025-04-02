namespace ChatApp.Model;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ConnectionId { get; set; }
    public string Nickname { get; set; }
    public string ChatRoomId { get; set; }
    public ChatRoom ChatRoom { get; set; }
    public List<ChatMessage> Messages { get; set; } = new();
}