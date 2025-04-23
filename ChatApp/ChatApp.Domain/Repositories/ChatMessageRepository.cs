using ChatApp.Domain.Interfaces;
using ChatApp.Model.Context;
using ChatApp.Model.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Domain.Repositories;

public class ChatMessageRepository : ARepository<ChatMessage>, IChatMessageRepository
{
    public ChatMessageRepository(ChatAppContext ctx) : base(ctx) { }

    public Task<IEnumerable<ChatMessage>> GetByRoomIdAsync(string roomId, int take = 50) =>
        Table.Where(m => m.ChatRoomId == roomId)
            .OrderByDescending(m => m.SentAt)
            .Take(take)
            .Include(m => m.User)                     // eager‑load sender when available
            .ToListAsync()
            .ContinueWith(t => (IEnumerable<ChatMessage>)t.Result);
}