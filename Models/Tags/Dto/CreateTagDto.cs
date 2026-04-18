using System.ComponentModel.DataAnnotations;

namespace backend.Models.Tags.Dto
{
    public class CreateTagDto
    {
        [Required]
        [MaxLength(25)]
        public string Title { get; set; }
        [Required]
        [MaxLength(250)]
        public string Description { get; set; }
    }
}
