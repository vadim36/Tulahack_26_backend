using backend.Data;
using backend.Models.Tags;
using backend.Models.Tags.Dto;
using backend.Models.Tags.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("tags")]
    public class TagController : ControllerBase
    {
        private readonly AppDbContext _context;
        public TagController(AppDbContext context) { 
            _context = context;
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpGet()]
        public async Task<ActionResult<AllTagsResponse>> GetAllTags()
        {
            var tags = await _context.Tags.ToListAsync();

            return Ok(new AllTagsResponse
            {
                Tags = tags
            });
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpPost("new")]
        public async Task<ActionResult<CreateTagResponse>> CreateTag([FromForm] CreateTagDto dto)
        {
            var newTag = new Tag { 
                Title = dto.Title,
                Description = dto.Description,
            };

            _context.Tags.Add(newTag);
            await _context.SaveChangesAsync();

            return Ok(new CreateTagResponse
            {
                Tag = newTag
            });
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpPut("update")]
        public async Task<ActionResult<UpdateTagResponse>> UpdateTag([FromForm] UpdateTagDto dto)
        {
            var tag = await _context.Tags.FirstOrDefaultAsync(x => x.Id == dto.Id);

            if (tag == null) {
                return NotFound(new { error = "Тэг не найден" });
            }

            var updateTag = new Tag
            {
                Id = dto.Id,
                Title = dto.Title != null ? dto.Title : tag.Title,
                Description = dto.Description != null ? dto.Description : tag.Description,
            };

            _context.Tags.Update(updateTag);
            await _context.SaveChangesAsync();

            return Ok(new UpdateTagResponse
            {
                Tag = updateTag
            });
        }

        [Authorize(Roles = "ActiveUser")]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteTag([FromForm] UpdateTagDto dto)
        {
            var tag = await _context.Tags.FirstOrDefaultAsync(x => x.Id == dto.Id);

            if (tag == null)
            {
                return NotFound(new { error = "Тэг не найден" });
            }

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
