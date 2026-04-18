using backend.Data;
using backend.Models.Chats;
using backend.Models.Chats.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Hubs
{ 
    [Authorize(Roles = "ActiveUser")]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;
        private static readonly Dictionary<string, string> _userConnections = new();

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId != null)
            {
                _userConnections[userId] = Context.ConnectionId;

                var user = await _context.Users.FindAsync(int.Parse(userId));
                if (user != null)
                {
                    user.IsOnline = true;
                    user.LastSeen = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await Clients.All.SendAsync("UserOnline", userId);
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = _userConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (userId != null)
            {
                _userConnections.Remove(userId);

                var user = await _context.Users.FindAsync(int.Parse(userId));
                if (user != null)
                {
                    user.IsOnline = false;
                    user.LastSeen = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await Clients.All.SendAsync("UserOffline", userId);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(Guid chatId, Guid sendToId, string content)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == Guid.Parse(userId));

            var message = new ChatMessage
            {
                ChatId = chatId,
                User = user,
                Content = content
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            var sendMessage = new ChatMessageResponse
            {
                Id = message.Id,
                AvatarPath = message.User.Questionary.ImagePath,
                Name = message.User.Name,
                Content = message.Content,
                isRead = message.isRead,
                IsDeleted = message.IsDeleted,
                CreatedAt = message.CreatedAt,
                UpdatedAt = message.UpdatedAt
            };

            if (_userConnections.TryGetValue(sendToId.ToString(), out var connectionId))
            {
                await Clients.Client(_userConnections[connectionId]).SendAsync("ReceiveMessage", sendMessage);
            }
           
        }

        public async Task RemoveMessage(Guid messageId, Guid sendToId, string content)
        {
            var message = await _context.ChatMessages.FirstOrDefaultAsync(x => x.Id == messageId);

            _context.ChatMessages.Remove(message);
            await _context.SaveChangesAsync();

            if (_userConnections.TryGetValue(sendToId.ToString(), out var connectionId))
            {
                await Clients.Client(_userConnections[connectionId]).SendAsync("RemoveMessage", messageId.ToString());
            }
        }

        public async Task MarkAsRead(Guid messageId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var message = await _context.ChatMessages.FindAsync(messageId);
            if (message != null && !message.isRead)
            {
                message.isRead = true;
                await _context.SaveChangesAsync();

                if (_userConnections.TryGetValue(message.User.Id.ToString(), out var connectionId))
                {
                    await Clients.Client(connectionId).SendAsync("MessageRead", messageId, userId);
                }
            }
        }

        public async Task TypingIndicator(Guid chatId, Guid sendToId, bool isTyping)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;


            if (_userConnections.TryGetValue(sendToId.ToString(), out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("UserTyping", chatId, userId, isTyping);
            }
        }
    }
}
