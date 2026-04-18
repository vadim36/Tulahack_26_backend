using backend.Controllers;
using backend.Models.Pets;
using backend.Models.PetsBookmarks;
using backend.Models.PetsBookmarks.Dto;
using backend.Models.PetsBookmarks.Response;
using backend.Models.PetsType;
using backend.Models.Tags;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests.Unit;

public class PetBookmarksControllerUnitTests
{
    [Fact]
    public async Task GetSavedPets_ReturnsOnlyCurrentUserBookmarks()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var userId = Guid.NewGuid();
        var pet = new Pet
        {
            UserId = Guid.NewGuid(),
            Name = "p",
            Description = "d",
            City = "c",
            Breed = "b",
            ImagePath = "",
            Tags = new List<Tag>(),
            PetType = new PetType { Name = "t", ImagePath = "" }
        };
        ctx.Pets.Add(pet);
        ctx.PetBookmarks.Add(new PetBookmark { UserId = userId, Pet = pet });
        ctx.PetBookmarks.Add(new PetBookmark { UserId = Guid.NewGuid(), Pet = pet });
        await ctx.SaveChangesAsync();

        var sut = new PetBookmarksController(ctx);
        sut.SetUser(userId);

        var result = await sut.GetSavedPets();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<UserSavedPets>(ok.Value);
        Assert.Single(body.Pets);
    }

    [Fact]
    public async Task BookmarkPet_AddsBookmark()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var userId = Guid.NewGuid();
        var pet = new Pet
        {
            UserId = Guid.NewGuid(),
            Name = "p",
            Description = "d",
            City = "c",
            Breed = "b",
            ImagePath = "",
            Tags = new List<Tag>(),
            PetType = new PetType { Name = "t", ImagePath = "" }
        };
        ctx.Pets.Add(pet);
        await ctx.SaveChangesAsync();

        var sut = new PetBookmarksController(ctx);
        sut.SetUser(userId);

        var result = await sut.BookmarkPet(new BookmarkDto { PetId = pet.Id });

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(1, await ctx.PetBookmarks.CountAsync(b => b.UserId == userId));
    }

    [Fact]
    public async Task UnbookmarkPet_WhenOwner_Removes()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var userId = Guid.NewGuid();
        var pet = new Pet
        {
            UserId = Guid.NewGuid(),
            Name = "p",
            Description = "d",
            City = "c",
            Breed = "b",
            ImagePath = "",
            Tags = new List<Tag>(),
            PetType = new PetType { Name = "t", ImagePath = "" }
        };
        ctx.Pets.Add(pet);
        var bm = new PetBookmark { UserId = userId, Pet = pet };
        ctx.PetBookmarks.Add(bm);
        await ctx.SaveChangesAsync();

        var sut = new PetBookmarksController(ctx);
        sut.SetUser(userId);

        var result = await sut.UnbookmarkPet(new UnbookmarkDto { Id = bm.Id });

        Assert.IsType<OkResult>(result);
        Assert.Equal(0, await ctx.PetBookmarks.CountAsync());
    }
}
