namespace backend.Models.Chats.Response
{
    public class FullChatResponse
    {
        public Guid Id { get; set; }
        public string AvatarPath { get; set; }
        public string Name { get; set; }
        public IEnumerable<ChatMessageResponse> Messages { get; set; }
    }
}
