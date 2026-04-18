using backend.Data;
using backend.Models.Pets;
using backend.Models.Pets.Dto;
using backend.Models.Pets.Response;
using backend.Models.PetsHealth;
using backend.Models.PetsType;
using backend.Models.Questionarys;
using backend.Models.Share;
using backend.Models.Tags;
using backend.Services.Image;
using backend.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("pets")]
    public class PetController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ImageService _imageService;
        private readonly IWebHostEnvironment _environment;
        public PetController(AppDbContext context, ImageService imageService, IWebHostEnvironment environment)
        {
            _context = context;
            _imageService = imageService;
            _environment = environment;
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpGet("me")]
        public async Task<ActionResult<UserPetsResponse>> GetUserPets()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var pets = await _context.Pets.Where(x => x.UserId == Guid.Parse(userId)).ToListAsync();

            return Ok(new UserPetsResponse
            {
                Pets = pets
            });
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpPost("new")]
        public async Task<ActionResult<CreatePetResponse>> CreatePet([FromForm] CreatePetDto dto)
        {
            var imagePath = "";
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

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

            var tags = new List<Tag>();

            foreach (var tagId in dto.TagIds)
            {
                var tag_data = await _context.Tags.FirstOrDefaultAsync(x => x.Id == Guid.Parse(tagId));

                if (tag_data != null)
                {
                    tags.Add(tag_data);
                }
            }

            var petType = await _context.PetTypes.FirstOrDefaultAsync(x => x.Id == dto.PetTypeId);

           

            var newPet = new Pet
            {
                UserId = Guid.Parse(userId),
                Name = dto.Name,
                Description = dto.Description,
                City = "",
                Breed = "",
                Tags = tags,
                ImagePath = imagePath,
                PetType = petType
            };

            var newHealth = new PetHealth
            {
                PetId = newPet.Id,
                Age = dto.Age,
                Weight = dto.Weight,
                Birthday = dto.Birthday,

                EnergyRating = dto.EnergyRating,
                FriendlyRating = dto.FriendlyRating,
                ObedienceRating = dto.ObedienceRating,
                HealthRating = dto.HealthRating,

                Description = dto.HealthDescription
            };

            newPet.PetHealth = newHealth;


            _context.PetCalendars.Add(new Models.PetCalendars.PetCalendar
            {
                UserId = Guid.Parse(userId)
            });
            _context.Pets.Add(newPet);
            await _context.SaveChangesAsync();

            return Ok(new CreatePetResponse
            {
                Pet = newPet
            });
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpPut("update")]
        public async Task<ActionResult<UpdatePetResponse>> UpdateTag([FromForm] UpdatePetDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var pet = await _context.Pets.FirstOrDefaultAsync(x => x.Id == dto.Id);

            if (pet == null)
            {
                return NotFound(new { error = "Питомец не найден" });
            }

            if (pet.UserId != Guid.Parse(userId))
            {
                return BadRequest(new { error = "Доступ запрещён" });
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

            var tags = new List<Tag>();

            if (dto.TagIds != null) {
                foreach (var tagId in dto.TagIds)
                {
                    var tag_data = await _context.Tags.FirstOrDefaultAsync(x => x.Id == Guid.Parse(tagId));

                    if (tag_data != null)
                    {
                        tags.Add(tag_data);
                    }
                }
            }

            var type = new PetType();

            if (dto.PetTypeId != null)
            {
                type = await _context.PetTypes.FirstOrDefaultAsync(x => x.Id == dto.PetTypeId);
            }

            var updatePet = new Pet
            {
                Id = dto.Id,
                Name = dto.Name != null ? dto.Name : pet.Name,
                Description = dto.Description != null ? dto.Description : pet.Description,
                Tags = tags.Count > 0 ? tags : pet.Tags,
                ImagePath = imagePath != "" ? imagePath : pet.ImagePath,
                PetType = dto.PetTypeId != null ? type : pet.PetType
            };

            var updateHealth = new PetHealth
            {
                Id = updatePet.PetHealth.Id,
                PetId = updatePet.PetHealth.PetId,

                Age = (int)(dto.Age != null ? dto.Age : updatePet.PetHealth.Age),
                Weight = (float)(dto.Weight != null ? dto.Weight : updatePet.PetHealth.Weight),
                Birthday = (DateOnly)(dto.Birthday != null ? dto.Birthday : updatePet.PetHealth.Birthday),

                EnergyRating = (float)(dto.EnergyRating != null ? dto.EnergyRating : updatePet.PetHealth.EnergyRating),
                FriendlyRating = (float)(dto.FriendlyRating != null ? dto.FriendlyRating : updatePet.PetHealth.FriendlyRating),
                ObedienceRating = (float)(dto.ObedienceRating != null ? dto.ObedienceRating : updatePet.PetHealth.ObedienceRating),
                HealthRating = (float)(dto.HealthRating != null ? dto.HealthRating : updatePet.PetHealth.HealthRating),

                Description = dto.HealthDescription != null ? dto.HealthDescription : updatePet.PetHealth.Description
            };

            updatePet.PetHealth = updateHealth;

            _context.Pets.Update(updatePet);
            await _context.SaveChangesAsync();

            return Ok(new UpdatePetResponse
            {
                Pet = updatePet
            });
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeletePet([FromForm] DeletePetDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var pet = await _context.Pets.FirstOrDefaultAsync(x => x.Id == dto.Id);

            if (pet == null)
            {
                return NotFound(new { error = "Питомец не найден" });
            }

            if (pet.UserId != Guid.Parse(userId))
            {
                return BadRequest(new { error = "Доступ запрещён" });
            }

            _imageService.DeleteImage(pet.ImagePath);

            _context.Pets.Remove(pet);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpGet("scroll")]
        public async Task<ActionResult<IEnumerable<UserPetsResponse>>> GetScroll()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userQuestionary = await _context.Questionarys.FirstOrDefaultAsync(x => x.UserId == Guid.Parse(userId));
            var pets = await _context.Pets.Where(x => x.UserId != Guid.Parse(userId))
                .Include(x => x.PetHealth)
                .Include(x => x.Tags)
                .ToListAsync();

            var tools = new XTools();

            var recommendedPets = pets
            .Where(p => tools.ApplyFilters(p, userQuestionary))
            .ToList();

            return Ok(recommendedPets);
        }
    }
}
