using ChatApp.Model.Context;
using ChatApp.Model.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Domain.Repositories;

public class MessageRepository(ChatAppContext context) : ARepository<Message>(context)
{
    private readonly ChatAppContext _context = context;
    
}