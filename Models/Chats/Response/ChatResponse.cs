namespace backend.Models.Chats.Response
{
    public class ChatResponse
    {
        public Guid Id { get; set; }
        public string AvatarPath { get; set; }
        public string Name { get; set; }
        public ChatMessageResponse? LastMessage { get; set; }
    }
}
