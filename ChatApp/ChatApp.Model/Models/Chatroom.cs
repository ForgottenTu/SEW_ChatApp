namespace ChatApp.Model.Models;

public class Chatroom
{
    public string Id { get; set; }
    public string Name { get; set; }
    
    public List<ApplicationUser> Users { get; set; }
    
}