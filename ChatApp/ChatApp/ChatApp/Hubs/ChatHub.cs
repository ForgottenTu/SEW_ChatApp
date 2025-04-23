using ChatApp.Domain.Interfaces;
using ChatApp.Model.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Hubs;

[Authorize]
public sealed class ChatHub : Hub
{
    private readonly IChatRoomRepository _rooms;
    private readonly IChatRoomMembershipRepository _members;
    private readonly IChatMessageRepository _messages;
    private readonly UserManager<ApplicationUser> _userManager; // null ⇒ no Identity

    public ChatHub(
        IChatRoomRepository rooms,
        IChatRoomMembershipRepository members,
        IChatMessageRepository messages,
        UserManager<ApplicationUser> userManager)
    {
        _rooms = rooms;
        _members = members;
        _messages = messages;
        _userManager = userManager;
    }

    /*────────────────────────────  bootstrap  ─────────────────────────────*/

    public override async Task OnConnectedAsync()
    {
        // Ensure a default room called “General” exists (Id = "1").
        if (!((await _rooms.GetAllAsync())?.Any() ?? false))
            await _rooms.AddAsync(new ChatRoom { Id = "1", Name = "General" });

        await base.OnConnectedAsync();
    }

    /*────────────────────────────  registration  ──────────────────────────*/

    /// <summary>For anonymous callers; authenticated users may also set a display name.</summary>
    public async Task Register(string nickname)
    {
        nickname = nickname.Trim();

        // Authenticated path – store nickname on the Identity record
        if (IsAuthenticated())
        {
            var user = await _userManager!.GetUserAsync(Context.User);
            if (user is null || string.IsNullOrWhiteSpace(nickname))
            {
                await Clients.Caller.SendAsync("Register", false);
                return;
            }

            user.DisplayName = nickname;
            await _userManager.UpdateAsync(user);
            await Clients.Caller.SendAsync("Register", true);
            return;
        }

        // Guest path – nickname must be unique among active members
        var nameTaken = (await _members.GetAllAsync() ?? Enumerable.Empty<ChatRoomMembership>())
            .Any(m => m.Nickname != null &&
                      m.Nickname.Equals(nickname, StringComparison.OrdinalIgnoreCase));

        var ok = !nameTaken && !nickname.Equals("system", StringComparison.OrdinalIgnoreCase);
        await Clients.Caller.SendAsync("Register", ok);
    }

    /*───────────────────────────  room listing  ───────────────────────────*/

    public async Task GetRooms()
    {
        var dict = (await _rooms.GetAllAsync() ?? Enumerable.Empty<ChatRoom>())
            .ToDictionary(r => r.Id, r => r.Name);
        await Clients.Caller.SendAsync("AllRooms", dict);
    }

    /*────────────────────────────  join / leave  ──────────────────────────*/

    public async Task JoinRoom(string roomId)
    {
        if (!await RoomExists(roomId)) return;

        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

        await _members.AddAsync(new ChatRoomMembership
        {
            ChatRoomId = roomId,
            ConnectionId = Context.ConnectionId,
            UserId = IsAuthenticated() ? _userManager!.GetUserId(Context.User) : null
        });

        var display = await GetDisplayNameAsync();
        await Clients.Group(roomId).SendAsync("ReceiveMessage", "System", $"{display} joined the room.");
        await SendRoomUserList(roomId);
    }

    public async Task LeaveRoom(string roomId)
    {
        var memberships = await _members.GetByConnectionIdAsync(Context.ConnectionId);
        var mem = memberships.FirstOrDefault(m => m.ChatRoomId == roomId);
        if (mem is null) return;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        await _members.DeleteAsync(mem);

        var display = await GetDisplayNameAsync();
        await Clients.Group(roomId).SendAsync("ReceiveMessage", "System", $"{display} left the room.");
        await SendRoomUserList(roomId);
        
        await Clients.Caller.SendAsync("LeftRoom", roomId);   // tell the leaver
        await Clients.Group(roomId)
            .SendAsync("ReceiveMessage", "System", $"{display} left the room.");
        await SendRoomUserList(roomId);
    }

    /*────────────────────────────  room admin  ────────────────────────────*/

    public async Task CreateRoom(string name)
    {
        if (await _rooms.GetByNameAsync(name) is not null)
        {
            await Clients.Caller.SendAsync("Error", "Chat room already exists.");
            return;
        }

        var room = await _rooms.AddAsync(new ChatRoom { Name = name });
        await Clients.All.SendAsync("RoomCreated", room!.Id, room.Name);
    }

    public async Task DeleteRoom(string roomId)
    {
        if (roomId == "1")
        {
            await Clients.Caller.SendAsync("Error", "Cannot delete the General chat room.");
            return;
        }

        if ((await _members.GetByRoomIdAsync(roomId)).Any())
        {
            await Clients.Caller.SendAsync("Error", "Chat room is not empty.");
            return;
        }

        var room = (await _rooms.GetAllAsync())?.FirstOrDefault(r => r.Id == roomId);
        if (room is null)
        {
            await Clients.Caller.SendAsync("Error", "Chat room does not exist.");
            return;
        }

        await _rooms.DeleteAsync(room);
        await Clients.All.SendAsync("RoomDeleted", roomId);
    }

    /*───────────────────────────  messaging  ──────────────────────────────*/

    public async Task SendMessage(string roomId, string message)
    {
        if (!await RoomExists(roomId))
        {
            await Clients.Caller.SendAsync("Error", "Chat room does not exist.");
            return;
        }

        var sender = await GetDisplayNameAsync();
        if (string.IsNullOrWhiteSpace(sender))
        {
            await Clients.Caller.SendAsync("Error", "You must register a nickname before sending messages.");
            return;
        }

        await _messages.AddAsync(new ChatMessage
        {
            ChatRoomId = roomId,
            UserId = IsAuthenticated() ? _userManager!.GetUserId(Context.User) : null,
            Sender = sender,
            Text = message
        });

        await Clients.Group(roomId).SendAsync("ReceiveMessage", sender, message);
    }

    /*─────────────────────────  disconnect cleanup  ───────────────────────*/

    public override async Task OnDisconnectedAsync(Exception? ex)
    {
        var memberships = await _members.GetByConnectionIdAsync(Context.ConnectionId);
        foreach (var m in memberships)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, m.ChatRoomId);
            await Clients.Group(m.ChatRoomId)
                .SendAsync("ReceiveMessage", "System", $"{await GetDisplayNameAsync()} disconnected.");
            await SendRoomUserList(m.ChatRoomId);
            await _members.DeleteAsync(m);
        }

        await base.OnDisconnectedAsync(ex);
    }

    /*────────────────────────────── helpers ───────────────────────────────*/

    private bool IsAuthenticated() =>
        _userManager is not null && Context.User?.Identity?.IsAuthenticated == true;

    private async Task<string> GetDisplayNameAsync()
    {
        if (IsAuthenticated())
            return (await _userManager!.GetUserAsync(Context.User))?.DisplayName ?? "Unknown";

        var nick = (await _members.GetByConnectionIdAsync(Context.ConnectionId))
            .FirstOrDefault()?.Nickname;
        return nick ?? "Unknown";
    }

    private async Task<bool> RoomExists(string id) =>
        (await _rooms.GetAllAsync())?.Any(r => r.Id == id) ?? false;

    private async Task SendRoomUserList(string roomId)
    {
        var list = (await _members.GetByRoomIdAsync(roomId))
            .Select(m => m.User?.DisplayName ?? m.Nickname ?? "Unknown")
            .ToList();
        await Clients.Group(roomId).SendAsync("RoomUsers", list);
    }
}