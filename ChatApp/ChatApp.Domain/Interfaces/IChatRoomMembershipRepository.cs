using ChatApp.Model.Models;

namespace ChatApp.Domain.Interfaces;

public interface IChatRoomMembershipRepository : IRepository<ChatRoomMembership>
{
    Task<IEnumerable<ChatRoomMembership>> GetByRoomIdAsync(string roomId);
    Task<IEnumerable<ChatRoomMembership>> GetByConnectionIdAsync(string connectionId);
}