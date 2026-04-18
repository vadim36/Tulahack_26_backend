using System.ComponentModel.DataAnnotations;

namespace backend.Models.PetsType.Dto
{
    public class UpdatePetTypeDto
    {
        [Required]
        public Guid Id { get; set; }
        public IFormFile? Image { get; set; }

        public string? Name { get; set; }
    }
}
