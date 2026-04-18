using System.ComponentModel.DataAnnotations;

namespace backend.Models.PetCalendars.Dto
{
    public class UpdateCalendarNoteDto
    {
        [Required]
        public Guid Id { get; set; }
        public Guid? CalendarId { get; set; }
        public DateTime? Time { get; set; }

        public string? Title { get; set; }
        public string? Description { get; set; }
    }
}
