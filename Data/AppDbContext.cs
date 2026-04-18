using backend.Models.Auth;
using backend.Models.PetCalendars;
using backend.Models.Pets;
using backend.Models.PetsBookmarks;
using backend.Models.PetsType;
using backend.Models.Questionarys;
using backend.Models.Tags;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Questionary> Questionarys { get; set; }

        public DbSet<Pet> Pets { get; set; }
        public DbSet<PetType> PetTypes { get; set; }
        public DbSet<PetCalendar> PetCalendars { get; set; }
        public DbSet<PetCalendarItem> PetCalendarItems { get; set; }
        public DbSet<PetCalendarNote> PetCalendarNotes { get; set; }
        public DbSet<PetBookmark> PetBookmarks { get; set; }
        public DbSet<Tag> Tags { get; set; }
    }
}
