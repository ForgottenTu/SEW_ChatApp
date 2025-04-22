using ChatApp.Domain.Interfaces;
using ChatApp.Model.Context;
using ChatApp.Model.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Domain.Repositories;

public class ChatroomRepository: ARepository<Chatroom>, IChatroomRepository
{
    public ChatroomRepository(ChatAppContext context) : base(context)
    {
        
    }


    public Task<List<Chatroom>> GetChatroomsByNameAsync(string name)
    {
        throw new NotImplementedException();
    }
}