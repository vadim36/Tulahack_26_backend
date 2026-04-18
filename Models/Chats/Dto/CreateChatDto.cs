using System.ComponentModel.DataAnnotations;

namespace backend.Models.Chats.Dto
{
    public class CreateChatDto
    {
        [Required]
        public Guid SecondUserId { get; set; }
        [Required]
        public Guid PetId { get; set; }
    }
}
