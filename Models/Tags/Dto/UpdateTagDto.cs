using System.ComponentModel.DataAnnotations;

namespace backend.Models.Tags.Dto
{
    public class UpdateTagDto
    {
        [Required]
        public Guid Id { get; set; }

        [MaxLength(25)]
        public string? Title { get; set; }
        [MaxLength(250)]
        public string? Description { get; set; }
    }
}
