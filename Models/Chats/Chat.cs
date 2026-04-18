using backend.Models.Auth;
using backend.Models.Pets;

namespace backend.Models.Chats
{
    public class Chat
    {
        public Guid Id { get; set; }
        public Pet Pet { get; set; }
        public User FirstUser { get; set; }
        public User SecondUser { get; set; }
        public IEnumerable<ChatMessage> Messages { get; set; } = Enumerable.Empty<ChatMessage>();
    }
}
