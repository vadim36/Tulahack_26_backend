using backend.Controllers;
using backend.Models.Tags;
using backend.Models.Tags.Dto;
using backend.Models.Tags.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests.Unit;

public class TagControllerUnitTests
{
    [Fact]
    public async Task GetAllTags_ReturnsSeededTags()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        ctx.Tags.Add(new Tag { Title = "a", Description = "d" });
        await ctx.SaveChangesAsync();

        var sut = new TagController(ctx);
        sut.SetUser(Guid.NewGuid());

        var result = await sut.GetAllTags();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<AllTagsResponse>(ok.Value);
        Assert.Single(body.Tags);
    }

    [Fact]
    public async Task CreateTag_PersistsAndReturnsOk()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var sut = new TagController(ctx);
        sut.SetUser(Guid.NewGuid());

        var result = await sut.CreateTag(new CreateTagDto { Title = "t", Description = "desc" });

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(1, await ctx.Tags.CountAsync());
    }

    [Fact]
    public async Task UpdateTag_WhenMissing_ReturnsNotFound()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var sut = new TagController(ctx);
        sut.SetUser(Guid.NewGuid());

        var result = await sut.UpdateTag(new UpdateTagDto { Id = Guid.NewGuid(), Title = "x", Description = "y" });

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task DeleteTag_WhenExists_ReturnsOk()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var tag = new Tag { Title = "del", Description = "d" };
        ctx.Tags.Add(tag);
        await ctx.SaveChangesAsync();

        var sut = new TagController(ctx);
        sut.SetUser(Guid.NewGuid());

        var result = await sut.DeleteTag(new UpdateTagDto { Id = tag.Id });

        Assert.IsType<OkResult>(result);
        Assert.Equal(0, await ctx.Tags.CountAsync());
    }
}
