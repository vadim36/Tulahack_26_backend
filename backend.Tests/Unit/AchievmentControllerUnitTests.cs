using backend.Controllers;
using backend.Models.Achievments;
using backend.Models.Achivments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests.Unit;

public class AchievmentControllerUnitTests
{
    [Fact]
    public async Task GetAllAchievments_ReturnsTemplateList()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var sut = new AchievmentController(ctx);
        sut.SetUser(Guid.NewGuid());

        var result = await sut.GetAllAchievments();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<TAchievment>>(ok.Value);
        Assert.Equal(14, list.Count());
    }

    [Fact]
    public async Task GetUserAchievments_ReturnsOnlyForUser()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var userId = Guid.NewGuid();
        ctx.Achievments.Add(new Achievment
        {
            UserId = userId,
            AchievmentId = 1,
            Name = "A",
            Description = "D"
        });
        ctx.Achievments.Add(new Achievment
        {
            UserId = Guid.NewGuid(),
            AchievmentId = 2,
            Name = "B",
            Description = "E"
        });
        await ctx.SaveChangesAsync();

        var sut = new AchievmentController(ctx);
        sut.SetUser(userId);

        var result = await sut.GetUserAchievments();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<Achievment>>(ok.Value);
        Assert.Single(list);
    }

    [Fact]
    public async Task GrantAchievment_PersistsRow()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var userId = Guid.NewGuid();
        var sut = new AchievmentController(ctx);
        sut.SetUser(userId);

        var result = await sut.GrantAchievment(1);

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(1, await ctx.Achievments.CountAsync(a => a.UserId == userId));
    }
}
