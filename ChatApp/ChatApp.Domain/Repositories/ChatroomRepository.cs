using ChatApp.Model.Context;
using ChatApp.Model.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Domain.Repositories;

public class ChatroomRepository(ChatAppContext context) : ARepository<Chatroom>(context)
{
    private readonly ChatAppContext _context = context;

    public async Task<List<Chatroom>> ReadAllAsync()
    {
        return await _context.Set<Chatroom>()
            .Include(s => s.Name)
            .ToListAsync();
    }
}