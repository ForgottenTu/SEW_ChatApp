using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Hubs;

public class ChatHub : Hub
{
    // ChatRoom: <roomId, roomName>
    private static readonly Dictionary<string, string> ChatRoom = new();

    // ChatRoomUsers: <connectionId, chatRoomId>
    private static readonly Dictionary<string, string> ChatRoomUsers = new();

    // RegisteredUsers: <connectionId, nickname>
    private static readonly Dictionary<string, string> RegisteredUsers = new();

    public ChatHub()
    {
        if (!ChatRoom.ContainsKey("1"))
        {
            ChatRoom.Add("1", "General");
        }
    }

    #region Registration

    public async Task Register(string nickname)
    {
        nickname = nickname.Trim();

        if (RegisteredUsers.Values.Any(n => string.Equals(n, nickname, StringComparison.OrdinalIgnoreCase)) || nickname.ToLower() == "system")
        {
            await Clients.Caller.SendAsync("Register", false);
            return;
        }

        RegisteredUsers[Context.ConnectionId] = nickname;
        await Clients.Caller.SendAsync("Register", true);
    }

    #endregion

    #region Rooms Leave Join
    
    public async Task JoinRoom(string chatRoomId)
    {
        if (ChatRoom.TryGetValue(chatRoomId, out var roomName))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomId);
            ChatRoomUsers[Context.ConnectionId] = chatRoomId;
            Clients.Group(chatRoomId).SendAsync("ReceiveMessage", "System", $"{RegisteredUsers[Context.ConnectionId]} has joined the room.");
            await UpdateRoomUsers(chatRoomId);
        }
    }

    public async Task LeaveRoom(string chatRoomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatRoomId);
        ChatRoomUsers.Remove(Context.ConnectionId);
        
        Clients.Group(chatRoomId).SendAsync("ReceiveMessage", "System", $"{RegisteredUsers[Context.ConnectionId]} Left the room.");
        await UpdateRoomUsers(chatRoomId);
    }

    public async Task GetRooms()
    {
        await Clients.Caller.SendAsync("AllRooms", ChatRoom);
    }

    private async Task UpdateRoomUsers(string chatRoomId)
    {
        var usersInRoom = ChatRoomUsers
            .Where(kvp => kvp.Value == chatRoomId)
            .Select(kvp => RegisteredUsers.TryGetValue(kvp.Key, out var nick) ? nick : "Unknown")
            .ToList();
        await Clients.Group(chatRoomId).SendAsync("RoomUsers", usersInRoom);
    }

    #endregion

    #region Create & Delete Room

    public async Task CreateRoom(string chatRoomName)
    {
        if (ChatRoom.ContainsValue(chatRoomName))
        {
            await Clients.Caller.SendAsync("Error", "Chat room already exists.");
        }
        else
        {
            var id = Guid.NewGuid().ToString();
            ChatRoom.Add(id, chatRoomName);
            await Clients.All.SendAsync("RoomCreated", id, chatRoomName);
        }
    }

    public async Task DeleteRoom(string chatRoomId)
    {
        if (chatRoomId == "1")
        {
            await Clients.Caller.SendAsync("Error", "Cannot delete the General chat room.");
        }
        else if (ChatRoomUsers.ContainsValue(chatRoomId))
        {
            await Clients.Caller.SendAsync("Error", $"Chat room '{ ChatRoom[chatRoomId] }' is not empty.");
        }
        else if (ChatRoom.ContainsKey(chatRoomId))
        {
            ChatRoom.Remove(chatRoomId);
            await Clients.All.SendAsync("RoomDeleted", chatRoomId);
        }
        else
        {
            await Clients.Caller.SendAsync("Error", "Chat room does not exist.");
        }
    }

    #endregion

    #region Messaging

    public async Task SendMessage(string message, string chatRoomId)
    {
        if (!RegisteredUsers.TryGetValue(Context.ConnectionId, out string? user))
        {
            await Clients.Caller.SendAsync("Error", "You must register a nickname before sending messages.");
            return;
        }

        if (ChatRoom.ContainsKey(chatRoomId))
        {
            await Clients.Group(chatRoomId).SendAsync("ReceiveMessage", user, message);
        }
        else
        {
            await Clients.Caller.SendAsync("Error", "Chat room does not exist.");
        }
    }

    #endregion

    #region Connection Leave Handler

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (ChatRoomUsers.TryGetValue(Context.ConnectionId, out var chatRoomId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatRoomId);
            ChatRoomUsers.Remove(Context.ConnectionId);
            await UpdateRoomUsers(chatRoomId);
        }

        if (RegisteredUsers.ContainsKey(Context.ConnectionId))
        {
            RegisteredUsers.Remove(Context.ConnectionId);
        }

        if (ChatRoomUsers.ContainsKey(Context.ConnectionId))
        {
            ChatRoomUsers.Remove(Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    #endregion
}