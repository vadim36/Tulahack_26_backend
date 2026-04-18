using backend.Models.Auth;

namespace backend.Models.Chats.Response
{
    public class ChatMessageResponse
    {
        public Guid Id { get; set; }
        public string AvatarPath { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public bool isRead { get; set; }
        public bool IsDeleted { get; set; }

        public Guid? ParentMessageId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
