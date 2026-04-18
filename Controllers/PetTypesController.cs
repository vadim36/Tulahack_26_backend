using backend.Data;
using backend.Models.PetsType;
using backend.Models.PetsType.Dto;
using backend.Services.Image;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("pets/types")]
    public class PetTypesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ImageService _imageService;
        private readonly IWebHostEnvironment _environment;
        public PetTypesController(AppDbContext context, ImageService imageService, IWebHostEnvironment environment)
        {
            _context = context;
            _imageService = imageService;
            _environment = environment;
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpGet()]
        public async Task<ActionResult<IEnumerable<PetType>>> GetAllTypes()
        {
            var pets = await _context.PetTypes.ToListAsync();

            return Ok(pets);
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpPost("new")]
        public async Task<ActionResult<PetType>> CreatePetType([FromForm] CreatePetTypeDto dto)
        {
            var imagePath = "";

            if (dto.Image != null)
            {
                try
                {
                    imagePath = await _imageService.SaveImageAsync(dto.Image);
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(new { error = ex.Message });
                }
            }

            var newPetType = new PetType
            {
                Name = dto.Name,
                ImagePath = imagePath
            };

            _context.PetTypes.Add(newPetType);
            await _context.SaveChangesAsync();

            return Ok(newPetType);
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpPut("update")]
        public async Task<ActionResult<PetType>> UpdateTag([FromForm] UpdatePetTypeDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var petType = await _context.PetTypes.FirstOrDefaultAsync(x => x.Id == dto.Id);

            if (petType == null)
            {
                return NotFound(new { error = "Тип питомца не найден" });
            }

            var imagePath = "";

            if (dto.Image != null)
            {
                try
                {
                    imagePath = await _imageService.SaveImageAsync(dto.Image);
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(new { error = ex.Message });
                }
            }

            var updatePetType = new PetType
            {
                Id = dto.Id,
                Name = dto.Name != null ? dto.Name : petType.Name,
                ImagePath = imagePath != null ? imagePath : petType.ImagePath
            };

            _context.PetTypes.Update(updatePetType);
            await _context.SaveChangesAsync();

            return Ok(updatePetType);
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeletePet([FromForm] DeletePetTypeDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var petType = await _context.PetTypes.FirstOrDefaultAsync(x => x.Id == dto.Id);

            if (petType == null)
            {
                return NotFound(new { error = "Тип питомца не найден" });
            }

            _imageService.DeleteImage(petType.ImagePath);

            _context.PetTypes.Remove(petType);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
