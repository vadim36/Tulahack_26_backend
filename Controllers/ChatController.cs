using backend.Data;
using backend.Models.Chats;
using backend.Models.Chats.Dto;
using backend.Models.Chats.Response;
using backend.Models.PetsBookmarks.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("chats")]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChatController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<ChatResponse>>> GetUserChats()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var chats = await _context.Chats.Where(x => x.FirstUser.Id == Guid.Parse(userId)
                || x.SecondUser.Id == Guid.Parse(userId)).Select(x =>
                new ChatResponse {
                    Id = x.Id,
                    AvatarPath = x.Pet.ImagePath,
                    Name = x.Pet.Name,
                    LastMessage = new ChatMessageResponse
                    {
                        Id = x.Messages.Last().Id,
                        AvatarPath = x.Messages.Last().User.Questionary.ImagePath,
                        Name = x.Messages.Last().User.Name,
                        Content = x.Messages.Last().Content,
                        isRead = x.Messages.Last().isRead,
                        IsDeleted = x.Messages.Last().IsDeleted,
                        CreatedAt = x.Messages.Last().CreatedAt,
                        UpdatedAt = x.Messages.Last().UpdatedAt
                    }
                }).ToListAsync();

            return Ok();
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpGet("{chatId}")]
        public async Task<ActionResult<FullChatResponse>> GetChat(string chatId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var chat = await _context.Chats.FirstOrDefaultAsync(x => x.Id == Guid.Parse(chatId));

            if (Guid.Parse(userId) != chat.FirstUser.Id && Guid.Parse(userId) != chat.SecondUser.Id)
            {
                return BadRequest(new { error = "Доступ запрещён" });
            }

            return Ok(new FullChatResponse
            {
                Id = chat.Id,
                AvatarPath = chat.Pet.ImagePath,
                Name = chat.Pet.Name,
                Messages = chat.Messages.Select(x => new ChatMessageResponse { 
                    Id = x.Id,
                    AvatarPath = x.User.Questionary.ImagePath,
                    Name = x.User.Name,
                    Content = x.Content,
                    isRead = x.isRead,
                    IsDeleted = x.IsDeleted,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                }).ToList()
            });
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpPost("new")]
        public async Task<ActionResult<Chat>> CreateNewChat([FromBody] CreateChatDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var firstUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == Guid.Parse(userId));
            var secondUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == dto.SecondUserId);
            var pet = await _context.Pets.FirstOrDefaultAsync(x => x.Id == dto.PetId);

            var newChat = new Chat
            {
                FirstUser = firstUser,
                SecondUser = secondUser,
                Pet = pet
            };

            _context.Chats.Add(newChat);
            await _context.SaveChangesAsync();

            return Ok(new ChatResponse
            {
                Id = newChat.Id,
                Name = newChat.Pet.Name,
                AvatarPath = newChat.Pet.ImagePath,
                LastMessage = new ChatMessageResponse
                {
                    Id = newChat.Messages.Last().Id,
                    AvatarPath = newChat.Messages.Last().User.Questionary.ImagePath,
                    Name = newChat.Messages.Last().User.Name,
                    Content = newChat.Messages.Last().Content,
                    IsDeleted = newChat.Messages.Last().IsDeleted,
                    isRead = newChat.Messages.Last().isRead,
                    CreatedAt = newChat.Messages.Last().CreatedAt,
                    UpdatedAt = newChat.Messages.Last().UpdatedAt
                }
            });
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpDelete("remove")]
        public async Task<IActionResult> DeleteChat([FromBody] string ChatId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var chat = await _context.Chats.FirstOrDefaultAsync(x => x.Id == Guid.Parse(ChatId));

            if (Guid.Parse(userId) != chat.FirstUser.Id && Guid.Parse(userId) != chat.SecondUser.Id)
            {
                return BadRequest(new { error = "Доступ запрещён" });
            }

            _context.ChatMessages.RemoveRange(chat.Messages);
            _context.Chats.Remove(chat);

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
