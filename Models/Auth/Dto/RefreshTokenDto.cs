using System.ComponentModel.DataAnnotations;

namespace backend.Models.Auth.Dto
{
    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
