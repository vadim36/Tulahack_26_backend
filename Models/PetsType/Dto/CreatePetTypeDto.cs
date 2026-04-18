using System.ComponentModel.DataAnnotations;

namespace backend.Models.PetsType.Dto
{
    public class CreatePetTypeDto
    {
        [Required]
        public IFormFile Image { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
