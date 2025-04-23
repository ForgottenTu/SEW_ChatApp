using ChatApp.Model.Models;

namespace ChatApp.Domain.Interfaces;

public interface IChatRoomRepository : IRepository<ChatRoom>
{
    Task<ChatRoom?>            GetByNameAsync(string name);
    Task<IEnumerable<ChatRoom>>GetWithMembersAsync();
}