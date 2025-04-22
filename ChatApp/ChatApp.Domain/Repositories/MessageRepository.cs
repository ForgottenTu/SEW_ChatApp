using ChatApp.Model.Context;
using ChatApp.Model.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ChatApp.Domain.Repositories;

public class MessageRepository(ChatAppContext context) : ARepository<Message>(context)
{
    private readonly ChatAppContext _context = context;


    public  async Task<Message> GetAllMessages()
    {
        return null;
    }
    
}