using System.ComponentModel.DataAnnotations;

namespace backend.Models.Tags.Dto
{
    public class DeleteTagDto
    {
        [Required]
        public Guid Id { get; set; }
    }
}
