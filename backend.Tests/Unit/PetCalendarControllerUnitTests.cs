using backend.Controllers;
using backend.Data;
using backend.Models.PetCalendars;
using backend.Models.PetCalendars.Dto;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace backend.Tests.Unit;

public class PetCalendarControllerUnitTests
{
    private static PetCalendarController CreateSut(AppDbContext ctx)
    {
        var (img, root) = ControllerTestHelper.CreateImageService();
        _ = root;
        var env = new Mock<IWebHostEnvironment>();
        env.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
        return new PetCalendarController(ctx, img, env.Object);
    }

    [Fact]
    public async Task GetCalendarItems_ReturnsItemsCollection()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var userId = Guid.NewGuid();
        var items = new List<PetCalendarItem>
        {
            new()
            {
                Name = "walk",
                ActionType = ActionType.Walk,
                Time = TimeOnly.Parse("08:00"),
                isActive = true
            }
        };
        var cal = new PetCalendar
        {
            UserId = userId,
            petCalendarItems = items,
            petCalendarNotes = new List<PetCalendarNote>()
        };
        ctx.PetCalendars.Add(cal);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx);
        sut.SetUser(userId);

        var result = await sut.GetCalendarItems(new CalendarDto { Id = cal.Id });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(items, ok.Value);
    }

    [Fact]
    public async Task CreateCalendarItem_Persists()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var calId = Guid.NewGuid();
        var sut = CreateSut(ctx);
        sut.SetUser(Guid.NewGuid());

        var result = await sut.CreateCalendarItem(new CreateCalendarItemDto
        {
            CalendarId = calId,
            Name = "feed",
            Time = TimeOnly.Parse("12:00"),
            ActionType = ActionType.Feed,
            isActive = true
        });

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateCalendarItem_WhenMissing_ReturnsNotFound()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var sut = CreateSut(ctx);
        sut.SetUser(Guid.NewGuid());

        var result = await sut.UpdateCalendarItem(new UpdateCalendarItemDto { Id = Guid.NewGuid(), Name = "x" });

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateCalendarItem_WhenExists_ReturnsOk()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var item = new PetCalendarItem
        {
            CalendarId = Guid.NewGuid(),
            Name = "old",
            Time = TimeOnly.Parse("09:00"),
            ActionType = ActionType.Walk,
            isActive = false
        };
        ctx.PetCalendarItems.Add(item);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx);
        sut.SetUser(Guid.NewGuid());

        var result = await sut.UpdateCalendarItem(new UpdateCalendarItemDto
        {
            Id = item.Id,
            Name = "new",
            isActive = true
        });

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task DeleteCalendarItem_RemovesRow()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var item = new PetCalendarItem
        {
            CalendarId = Guid.NewGuid(),
            Name = "x",
            Time = TimeOnly.Parse("10:00"),
            ActionType = ActionType.Action,
            isActive = true
        };
        ctx.PetCalendarItems.Add(item);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx);
        sut.SetUser(Guid.NewGuid());

        var result = await sut.DeleteCalendarItem(new DeleteCalendarItemDto { Id = item.Id });

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task CreateCalendarNote_Persists()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var sut = CreateSut(ctx);
        sut.SetUser(Guid.NewGuid());

        var result = await sut.CreateCalendarNote(new CreateCalendarNoteDto
        {
            CalendarId = Guid.NewGuid(),
            Title = "vet",
            Time = DateTime.UtcNow,
            Description = "visit"
        });

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateCalendarNote_WhenMissing_ReturnsNotFound()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var sut = CreateSut(ctx);
        sut.SetUser(Guid.NewGuid());

        var result = await sut.UpdateCalendarNote(new UpdateCalendarNoteDto { Id = Guid.NewGuid(), Title = "t" });

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task DeleteCalendarNotes_WhenExists_ReturnsOk()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var note = new PetCalendarNote
        {
            CalendarId = Guid.NewGuid(),
            Title = "n",
            Time = DateTime.UtcNow,
            Description = "d"
        };
        ctx.PetCalendarNotes.Add(note);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx);
        sut.SetUser(Guid.NewGuid());

        var result = await sut.DeleteCalendarNotes(new DeleteCalendarNoteDto { Id = note.Id });

        Assert.IsType<OkResult>(result);
    }
}
