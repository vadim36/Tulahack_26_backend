using backend.Data;
using backend.Models.PetCalendars;
using backend.Models.PetCalendars.Dto;
using backend.Models.PetsType;
using backend.Services.Image;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("pets/calendars")]
    public class PetCalendarController : ControllerBase
    {
        private readonly AppDbContext _context;
        public PetCalendarController(AppDbContext context, ImageService imageService, IWebHostEnvironment environment)
        {
            _context = context;
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpGet("items")]
        public async Task<ActionResult<IEnumerable<PetCalendarItem>>> GetCalendarItems([FromBody] CalendarDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var calendar = await _context.PetCalendars.FirstOrDefaultAsync(x => x.UserId == Guid.Parse(userId) && x.Id == dto.Id);

            return Ok(calendar.petCalendarItems);
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpPost("items/new")]
        public async Task<ActionResult<PetCalendarItem>> CreateCalendarItem([FromBody] CreateCalendarItemDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var newItem = new PetCalendarItem
            {
                CalendarId = dto.CalendarId,
                Name = dto.Name,
                ActionType = dto.ActionType,
                Time = dto.Time
            };

            _context.PetCalendarItems.Add(newItem);
            await _context.SaveChangesAsync();

            return Ok(newItem);
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpPut("items/update")]
        public async Task<ActionResult<PetCalendarItem>> UpdateCalendarItem([FromBody] UpdateCalendarItemDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var updateItem = await _context.PetCalendarItems.FirstOrDefaultAsync(x => x.Id == dto.Id);

            if (updateItem == null)
            {
                return NotFound(new { error = "Элемент не найден" });
            }

            updateItem.Name = dto.Name != null ? dto.Name : updateItem.Name;
            updateItem.ActionType = (ActionType)(dto.ActionType != null ? dto.ActionType : updateItem.ActionType);
            updateItem.Time = (TimeOnly)(dto.Time != null ? dto.Time : updateItem.Time);
            updateItem.isActive = (bool)(dto.isActive != null ? dto.isActive : updateItem.isActive);

            _context.PetCalendarItems.Update(updateItem);
            await _context.SaveChangesAsync();

            return Ok(updateItem);
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpDelete("items/remove")]
        public async Task<IActionResult> DeleteCalendarItem([FromBody] DeleteCalendarItemDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var calendarItem = await _context.PetCalendarItems.FirstOrDefaultAsync(x => x.Id == dto.Id);

            if (calendarItem == null)
            {
                return NotFound(new { error = "Элемент не найден" });
            }

            _context.PetCalendarItems.Remove(calendarItem);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpGet("notes")]
        public async Task<ActionResult<IEnumerable<PetCalendarNote>>> GetCalendarNotes([FromBody] CalendarDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var calendar = await _context.PetCalendars.FirstOrDefaultAsync(x => x.UserId == Guid.Parse(userId) && x.Id == dto.Id);

            return Ok(calendar.petCalendarNotes);
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpPost("notes/new")]
        public async Task<ActionResult<PetCalendarNote>> CreateCalendarNote([FromBody] CreateCalendarNoteDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var newItem = new PetCalendarNote
            {
                CalendarId = dto.CalendarId,
                Title = dto.Title,
                Time = dto.Time,
                Description = dto.Description,
            };

            _context.PetCalendarNotes.Add(newItem);
            await _context.SaveChangesAsync();

            return Ok(newItem);
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpPut("notes/update")]
        public async Task<ActionResult<PetCalendarNote>> UpdateCalendarNote([FromBody] UpdateCalendarNoteDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var updateNote = await _context.PetCalendarNotes.FirstOrDefaultAsync(x => x.Id == dto.Id);

            if (updateNote == null)
            {
                return NotFound(new { error = "Элемент не найден" });
            }

            updateNote.Title = dto.Title != null ? dto.Title : updateNote.Title;
            updateNote.Description = dto.Description != null ? dto.Description : updateNote.Description;
            updateNote.Time = (DateTime)(dto.Time != null ? dto.Time : updateNote.Time);

            _context.PetCalendarNotes.Update(updateNote);
            await _context.SaveChangesAsync();

            return Ok(updateNote);
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpDelete("notes/remove")]
        public async Task<IActionResult> DeleteCalendarNotes([FromBody] DeleteCalendarNoteDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var calendarNotes = await _context.PetCalendarNotes.FirstOrDefaultAsync(x => x.Id == dto.Id);

            if (calendarNotes == null)
            {
                return NotFound(new { error = "Элемент не найден" });
            }

            _context.PetCalendarNotes.Remove(calendarNotes);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
