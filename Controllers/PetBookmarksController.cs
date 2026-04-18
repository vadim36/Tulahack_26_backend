using backend.Data;
using backend.Models.PetsBookmarks;
using backend.Models.PetsBookmarks.Dto;
using backend.Models.PetsBookmarks.Response;
using backend.Services.Image;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("pets/bookmarks")]
    public class PetBookmarksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PetBookmarksController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpGet()]
        public async Task<ActionResult<UserSavedPets>> GetSavedPets()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var pets = await _context.PetBookmarks.Where(x => x.UserId == Guid.Parse(userId)).ToListAsync();

            return Ok(new UserSavedPets
            {
                Pets = pets
            });
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpPost("save")]
        public async Task<ActionResult<BookmarkResponse>> BookmarkPet([FromBody] BookmarkDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var pet = await _context.Pets.FirstOrDefaultAsync(x => x.Id == dto.PetId);

            var bookmarkPet = new PetBookmark
            {
                UserId = Guid.Parse(userId),
                Pet = pet
            };

            _context.PetBookmarks.Add(bookmarkPet);
            await _context.SaveChangesAsync();

            return Ok(new BookmarkResponse
            {
                Pet = bookmarkPet
            });
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpDelete("remove")]
        public async Task<IActionResult> UnbookmarkPet([FromBody] UnbookmarkDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var pet = await _context.PetBookmarks.FirstOrDefaultAsync(x => x.Id == dto.Id);

            if (Guid.Parse(userId) != pet.UserId)
            {
                return BadRequest(new { error = "Доступ запрещён" });
            }

            _context.PetBookmarks.Remove(pet);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
