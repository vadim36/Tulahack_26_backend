using backend.Controllers;
using backend.Data;
using backend.Models.Auth;
using backend.Models.PetsType;
using backend.Models.Questionarys;
using backend.Models.Questionarys.Dto;
using backend.Models.Share;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace backend.Tests.Unit;

public class QuestionaryControllerUnitTests
{
    private static QuestionaryController CreateSut(AppDbContext ctx, out string root)
    {
        var (img, r) = ControllerTestHelper.CreateImageService();
        root = r;
        var env = new Mock<IWebHostEnvironment>();
        env.Setup(e => e.WebRootPath).Returns(r);
        return new QuestionaryController(ctx, img, env.Object);
    }

    private static void TryDelete(string root)
    {
        try
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
        catch { /* ignore */ }
    }

    private static User SeedUser(AppDbContext ctx, Guid id)
    {
        var u = new User
        {
            Id = id,
            Email = $"{id:N}@q.test",
            PasswordHash = "x",
            Name = "user",
            Role = "User",
            SubscriptionTier = "Free",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        ctx.Users.Add(u);
        return u;
    }

    [Fact]
    public async Task GetAvatar_ReturnsStoredPath()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var userId = Guid.NewGuid();
        SeedUser(ctx, userId);
        ctx.Questionarys.Add(new Questionary
        {
            UserId = userId,
            ImagePath = "/uploads/a.png",
            Name = "n",
            PhoneNumber = "+10000000000",
            UserGender = Gender.Women,
            Age = 20,
            Bio = "b",
            City = "c",
            AllergicToPets = new List<PetType>(),
            WantToPets = new List<PetType>(),
            PetGender = Gender.Women,
            ageFrom = 0,
            ageTo = 99
        });
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx, out var root);
        try
        {
            sut.SetUser(userId, role: "ActiveUser");
            var result = await sut.GetAvatarUrl();
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal("/uploads/a.png", ok.Value);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public async Task GetQuestionary_ReturnsEntity()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var userId = Guid.NewGuid();
        SeedUser(ctx, userId);
        var q = new Questionary
        {
            UserId = userId,
            ImagePath = "",
            Name = "n",
            PhoneNumber = "+10000000000",
            UserGender = Gender.Women,
            Age = 20,
            Bio = "b",
            City = "c",
            AllergicToPets = new List<PetType>(),
            WantToPets = new List<PetType>(),
            PetGender = Gender.Women,
            ageFrom = 0,
            ageTo = 99
        };
        ctx.Questionarys.Add(q);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx, out var root);
        try
        {
            sut.SetUser(userId, role: "ActiveUser");
            var result = await sut.GetQuestionary();
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(q, ok.Value);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public async Task CreateQuestionary_AssignsActiveUserRole()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var userId = Guid.NewGuid();
        var user = SeedUser(ctx, userId);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx, out var root);
        try
        {
            sut.SetUser(userId, role: "User");
            var dto = new CreateQuestionaryDto
            {
                UserId = userId,
                Image = ControllerTestHelper.CreateFakeJpegFormFile(),
                Name = "Ann",
                PhoneNumber = "+10000000001",
                UserGender = Gender.Women,
                Age = 25,
                Bio = "bio",
                City = "Town",
                AllergicToPetIds = Array.Empty<string>(),
                WantToPetIds = Array.Empty<string>(),
                PetGender = Gender.Man,
                ageFrom = 1,
                ageTo = 15
            };

            var result = await sut.CreateQuestionary(dto);

            Assert.IsType<OkObjectResult>(result.Result);
            await ctx.Entry(user).ReloadAsync();
            Assert.Equal("ActiveUser", user.Role);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public async Task UpdateQuestionary_WhenMissing_ReturnsNotFound()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var userId = Guid.NewGuid();
        SeedUser(ctx, userId);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx, out var root);
        try
        {
            sut.SetUser(userId, role: "ActiveUser");
            var result = await sut.UpdateQuestionary(new UpdateQuestionaryDto
            {
                UserId = Guid.NewGuid(),
                AllergicToPetIds = Array.Empty<string>(),
                WantToPetIds = Array.Empty<string>()
            });
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
        finally
        {
            TryDelete(root);
        }
    }
}
