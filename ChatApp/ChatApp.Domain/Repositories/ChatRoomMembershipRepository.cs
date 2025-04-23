using ChatApp.Domain.Interfaces;
using ChatApp.Model.Context;
using ChatApp.Model.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Domain.Repositories;

public class ChatRoomMembershipRepository
    : ARepository<ChatRoomMembership>, IChatRoomMembershipRepository
{
    public ChatRoomMembershipRepository(ChatAppContext ctx) : base(ctx)
    {
    }


    public Task<IEnumerable<ChatRoomMembership>> GetByRoomIdAsync(string roomId) =>
        Table.Where(m => m.ChatRoomId == roomId)
            .Include(m => m.User) // 🔸 add this
            .ToListAsync()
            .ContinueWith(t => (IEnumerable<ChatRoomMembership>)t.Result);

    public Task<IEnumerable<ChatRoomMembership>> GetByConnectionIdAsync(string cid) =>
        Table.Where(m => m.ConnectionId == cid)
            .Include(m => m.User) // 🔸 add this
            .ToListAsync()
            .ContinueWith(t => (IEnumerable<ChatRoomMembership>)t.Result);
}