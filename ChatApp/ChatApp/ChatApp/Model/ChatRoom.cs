using ChatApp.Config;

namespace ChatApp.Model;

public class ChatRoom
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public List<User> Users { get; set; } = new();
    public List<ChatMessage> Messages { get; set; } = new();
}