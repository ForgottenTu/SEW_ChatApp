using ChatApp.Model.Models;

namespace ChatApp.Domain.Interfaces;

public interface IChatroomRepository : IRepository<Chatroom>
{
    Task<List<Chatroom>> GetChatroomsByNameAsync(string name); 

}