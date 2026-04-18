using backend.Controllers;
using backend.Data;
using backend.Models.PetsType;
using backend.Models.PetsType.Dto;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Tests.Unit;

public class PetTypesControllerUnitTests
{
    private static PetTypesController CreateSut(AppDbContext ctx, out string tempRoot)
    {
        var (img, root) = ControllerTestHelper.CreateImageService();
        tempRoot = root;
        var env = new Mock<IWebHostEnvironment>();
        env.Setup(e => e.WebRootPath).Returns(root);
        return new PetTypesController(ctx, img, env.Object);
    }

    [Fact]
    public async Task GetAllTypes_ReturnsSeededRows()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        ctx.PetTypes.Add(new PetType { Name = "Dog", ImagePath = "" });
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx, out var root);
        try
        {
            sut.SetUser(Guid.NewGuid());
            var result = await sut.GetAllTypes();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<PetType>>(ok.Value);
            Assert.Single(list);
        }
        finally
        {
            TryDeleteDir(root);
        }
    }

    [Fact]
    public async Task CreatePetType_WithImage_ReturnsOk()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var sut = CreateSut(ctx, out var root);
        try
        {
            sut.SetUser(Guid.NewGuid());
            var result = await sut.CreatePetType(new CreatePetTypeDto
            {
                Name = "Cat",
                Image = ControllerTestHelper.CreateFakeJpegFormFile()
            });

            Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(1, await ctx.PetTypes.CountAsync());
        }
        finally
        {
            TryDeleteDir(root);
        }
    }

    [Fact]
    public async Task UpdateTag_WhenMissing_ReturnsNotFound()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var sut = CreateSut(ctx, out var root);
        try
        {
            sut.SetUser(Guid.NewGuid());
            var result = await sut.UpdateTag(new UpdatePetTypeDto { Id = Guid.NewGuid(), Name = "x" });
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
        finally
        {
            TryDeleteDir(root);
        }
    }

    [Fact]
    public async Task DeletePet_WhenExists_ReturnsOk()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var type = new PetType { Name = "t", ImagePath = "" };
        ctx.PetTypes.Add(type);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx, out var root);
        try
        {
            sut.SetUser(Guid.NewGuid());
            var result = await sut.DeletePet(new DeletePetTypeDto { Id = type.Id });
            Assert.IsType<OkResult>(result);
            Assert.Equal(0, ctx.PetTypes.Count());
        }
        finally
        {
            TryDeleteDir(root);
        }
    }

    private static void TryDeleteDir(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
        catch
        {
            /* тестовая уборка — игнорируем блокировки ОС */
        }
    }
}
