using backend.Data;
using backend.Models.Achievments;
using backend.Models.Achivments;
using backend.Models.Pets.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("achievments")]
    public class AchievmentController : ControllerBase
    {
        private readonly AppDbContext _context;
        public IEnumerable<TAchievment> achievments = new List<TAchievment>()
        {
            new TAchievment {
                Id = 1,
                Name = "Мусорщик",
                Description = "Просмотреть 50 анкет за день"
            },
            new TAchievment {
                Id = 2,
                Name = "Котолюб",
                Description = "Лайкнуть 10 кошек подряд"
            },
            new TAchievment {
                Id = 3,
                Name = "Собачник",
                Description = "Лайкнуть 10 собак подряд "
            },
            new TAchievment {
                Id = 4,
                Name = "Дом, милый дом!",
                Description = "Приютите питомца"
            },
            new TAchievment {
                Id = 5,
                Name = "Мелочь пузатая",
                Description = "Перевести любую сумму (даже 1 ₽)"
            },
            new TAchievment {
                Id = 6,
                Name = "Кормилец",
                Description = "Оплатить корм на месяц (примерно 500 ₽)"
            },
            new TAchievment {
                Id = 7,
                Name = "Доктор Хаус",
                Description = "Оплатить лечение/прививку"
            },
            new TAchievment {
                Id = 8,
                Name = "Меценат",
                Description = "Проспонсировать 5 разных животных"
            },
            new TAchievment {
                Id = 9,
                Name = "Неприкасаемый",
                Description = "Спонсировать животное с инвалидностью"
            },
            new TAchievment {
                Id = 10,
                Name = "Крёстный Отец",
                Description = "Стать постоянным спонсором животного (>1000 ₽/мес)"
            },
            new TAchievment {
                Id = 11,
                Name = "Консильери",
                Description = "Помочь пристроить 3 животных (дать совет/репост)"
            },
            new TAchievment {
                Id = 12,
                Name = "Капо",
                Description = "Привести в сервис 10 друзей по реферальной ссылке"
            },
            new TAchievment {
                Id = 13,
                Name = "Георгий :3",
                Description = "Получите рукопожатие от Георгия"
            },
            new TAchievment {
                Id = 14,
                Name = "Дон",
                Description = "Получить все ачивки из всех разделов"
            },
        };

        public AchievmentController(AppDbContext context) { 
            _context = context;
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpGet()]
        public async Task<ActionResult<IEnumerable<Achievment>>> GetAllAchievments()
        {
            return Ok(achievments);
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<Achievment>>> GetUserAchievments()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var achievments = await _context.Achievments.Where(x => x.UserId == Guid.Parse(userId)).ToListAsync();

            return Ok(achievments);
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpPost("grant")]
        public async Task<ActionResult<IEnumerable<Achievment>>> GrantAchievment(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var achievment = achievments.FirstOrDefault(x => x.Id == id);

            var newAchievment = new Achievment
            {
                AchievmentId = id,
                Name = achievment.Name,
                Description = achievment.Description,
                UserId = Guid.Parse(userId)
            };

            _context.Achievments.Add(newAchievment);
            await _context.SaveChangesAsync();

            return Ok(achievments);
        }
    }
}
