using backend.Data;
using backend.Models.Auth;
using backend.Models.PetsType;
using backend.Models.Questionarys;
using backend.Models.Questionarys.Dto;
using backend.Services.Image;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("questionary")]
    public class QuestionaryController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ImageService _imageService;
        private readonly IWebHostEnvironment _environment;
        public QuestionaryController(AppDbContext context, ImageService imageService, IWebHostEnvironment environment)
        {
            _context = context;
            _imageService = imageService;
            _environment = environment;
        }

        [Authorize]
        [HttpGet("avatar")]
        public async Task<ActionResult<string>> GetAvatarUrl()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var questionary = await _context.Questionarys.FirstOrDefaultAsync(x => x.UserId == Guid.Parse(userId));

            return Ok(questionary.ImagePath);
        }

        [Authorize]
        [HttpGet()]
        public async Task<ActionResult<Questionary>> GetQuestionary()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var questionary = await _context.Questionarys.FirstOrDefaultAsync(x => x.UserId == Guid.Parse(userId));

            return Ok(questionary);
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<ActionResult<Questionary>> CreateQuestionary([FromForm] CreateQuestionaryDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == Guid.Parse(userId));

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

            var allergicToPets = new List<PetType>();

            foreach (var petId in dto.AllergicToPetIds)
            {
                var pet_data = await _context.PetTypes.FirstOrDefaultAsync(x => x.Id == Guid.Parse(petId));

                if (pet_data != null)
                {
                    allergicToPets.Add(pet_data);
                }
            }

            var wantToPets = new List<PetType>();

            foreach (var petId in dto.WantToPetIds)
            {
                var pet_data = await _context.PetTypes.FirstOrDefaultAsync(x => x.Id == Guid.Parse(petId));

                if (pet_data != null)
                {
                    wantToPets.Add(pet_data);
                }
            }

            var newQuestionary = new Questionary
            {
                UserId = dto.UserId,
                ImagePath = imagePath,
                Name = dto.Name,
                PhoneNumber = dto.PhoneNumber,
                UserGender = dto.UserGender,
                Age = dto.Age,
                Bio = dto.Bio,
                City = dto.City,
                AllergicToPets = allergicToPets,
                WantToPets = wantToPets,
                PetGender = dto.PetGender,
                ageFrom = dto.ageFrom,
                ageTo = dto.ageTo
            };

            user.Questionary = newQuestionary;
            user.Role = "ActiveUser";
            _context.Users.Update(user);
            _context.Questionarys.Add(newQuestionary);
            await _context.SaveChangesAsync();

            return Ok(newQuestionary);
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpPut("update")]
        public async Task<ActionResult<Questionary>> UpdateQuestionary([FromForm] UpdateQuestionaryDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == Guid.Parse(userId));
            var questionary = await _context.Questionarys.FirstOrDefaultAsync(x => x.UserId == dto.UserId);

            if (questionary == null) { 
                return NotFound(new { error = "Анкета не найдена" });
            }

            if (questionary.UserId != dto.UserId) 
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

            var allergicToPets = new List<PetType>();

            foreach (var petId in dto.AllergicToPetIds)
            {
                var pet_data = await _context.PetTypes.FirstOrDefaultAsync(x => x.Id == Guid.Parse(petId));

                if (pet_data != null)
                {
                    allergicToPets.Add(pet_data);
                }
            }

            var isWantToPets = dto.WantToPetIds != null ? true : false;
            var isAllergicToPets = dto.WantToPetIds != null ? true : false;

            var wantToPets = new List<PetType>();

            foreach (var petId in dto.WantToPetIds)
            {
                var pet_data = await _context.PetTypes.FirstOrDefaultAsync(x => x.Id == Guid.Parse(petId));

                if (pet_data != null)
                {
                    wantToPets.Add(pet_data);
                }
            }

            var updateQuestionary = new Questionary
            {
                UserId = dto.UserId,
                ImagePath = imagePath != "" ? imagePath : questionary.ImagePath,
                Name = dto.Name != null ? dto.Name : questionary.Name,
                UserGender = (Models.Share.Gender)(dto.UserGender != null ? dto.UserGender : questionary.UserGender),
                Age = (int)(dto.Age != null ? dto.Age : questionary.Age),
                Bio = dto.Bio != null ? dto.Bio : questionary.Bio,
                City = dto.City != null ? dto.City : questionary.City,
                AllergicToPets = isAllergicToPets ? allergicToPets : questionary.AllergicToPets,
                WantToPets = isWantToPets ? wantToPets : questionary.WantToPets,
                PetGender = (Models.Share.Gender)(dto.PetGender != null ? dto.PetGender : questionary.PetGender),
                ageFrom = (int)(dto.ageFrom != null ? dto.ageFrom : questionary.ageFrom),
                ageTo = (int)(dto.ageTo != null ? dto.ageTo : questionary.ageTo),
            };

            user.Questionary = updateQuestionary;

            _context.Users.Update(user);
            _context.Questionarys.Update(updateQuestionary);
            await _context.SaveChangesAsync();

            return Ok(updateQuestionary);
        }
    }
}
