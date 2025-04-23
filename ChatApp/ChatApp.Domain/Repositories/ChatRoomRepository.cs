using ChatApp.Domain.Interfaces;
using ChatApp.Model.Context;
using ChatApp.Model.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Domain.Repositories;

public class ChatRoomRepository : ARepository<ChatRoom>, IChatRoomRepository
{
    public ChatRoomRepository(ChatAppContext ctx) : base(ctx) { }

    public Task<ChatRoom?> GetByNameAsync(string name) =>
        Table.FirstOrDefaultAsync(r => r.Name == name);

    public async Task<IEnumerable<ChatRoom>> GetWithMembersAsync()
    {
        return await Table.Include(r => r.Members).ToListAsync();
    }

}