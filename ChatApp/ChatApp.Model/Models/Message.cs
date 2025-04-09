namespace ChatApp.Model.Models;

public class Message
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string Text { get; set; }
    public DateTime Time { get; set; }
    
    public Chatroom Chatroom { get; set; }
    
}