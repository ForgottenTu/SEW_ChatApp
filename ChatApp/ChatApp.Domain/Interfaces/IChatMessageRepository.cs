using ChatApp.Model.Models;

namespace ChatApp.Domain.Interfaces;

public interface IChatMessageRepository : IRepository<ChatMessage>
{
    /// <summary>Latest messages for a room (default 50).</summary>
    Task<IEnumerable<ChatMessage>> GetByRoomIdAsync(string roomId, int take = 50);
}