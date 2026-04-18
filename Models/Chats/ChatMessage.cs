using backend.Models.Auth;

namespace backend.Models.Chats
{
    public class ChatMessage
    {
        public Guid Id { get; set; }
        public Guid ChatId { get; set; }
        public User User { get; set; }
        public string Content { get; set; }
        public bool isRead { get; set; } = false;
        public bool IsDeleted { get; set; } = false;

        public Guid? ParentMessageId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
