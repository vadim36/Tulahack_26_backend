using backend.Controllers;
using backend.Data;
using backend.Models.Auth;
using backend.Models.Pets;
using backend.Models.Pets.Dto;
using backend.Models.Pets.Response;
using backend.Models.Questionarys;
using backend.Models.PetsHealth;
using backend.Models.PetsType;
using backend.Models.Share;
using backend.Models.Tags;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace backend.Tests.Unit;

public class PetControllerUnitTests
{
    private static PetController CreateSut(AppDbContext ctx, out string root)
    {
        var (img, r) = ControllerTestHelper.CreateImageService();
        root = r;
        var env = new Mock<IWebHostEnvironment>();
        env.Setup(e => e.WebRootPath).Returns(r);
        return new PetController(ctx, img, env.Object);
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

    [Fact]
    public async Task GetUserPets_ReturnsEmptyList()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var userId = Guid.NewGuid();
        var sut = CreateSut(ctx, out var root);
        try
        {
            sut.SetUser(userId);
            var result = await sut.GetUserPets();
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var body = Assert.IsType<UserPetsResponse>(ok.Value);
            Assert.Empty(body.Pets);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public async Task CreatePet_WithValidData_ReturnsOk()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var userId = Guid.NewGuid();
        var type = new PetType { Name = "dog", ImagePath = "" };
        var tag = new Tag { Title = "cute", Description = "d" };
        ctx.PetTypes.Add(type);
        ctx.Tags.Add(tag);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx, out var root);
        try
        {
            sut.SetUser(userId);
            var dto = new CreatePetDto
            {
                Image = ControllerTestHelper.CreateFakeJpegFormFile(),
                PetTypeId = type.Id,
                Name = "Buddy",
                Description = "Nice dog",
                TagIds = new[] { tag.Id.ToString() },
                Age = 3,
                PetGender = Gender.Women,
                Weight = 10f,
                Birthday = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-3)),
                EnergyRating = 4f,
                FriendlyRating = 5f,
                ObedienceRating = 3f,
                HealthRating = 4f,
                HealthDescription = "Healthy"
            };

            var result = await sut.CreatePet(dto);

            Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(1, ctx.Pets.Count(p => p.UserId == userId));
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public async Task UpdateTag_WhenPetMissing_ReturnsNotFound()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var sut = CreateSut(ctx, out var root);
        try
        {
            sut.SetUser(Guid.NewGuid());
            var type = new PetType { Name = "t", ImagePath = "" };
            ctx.PetTypes.Add(type);
            await ctx.SaveChangesAsync();

            var result = await sut.UpdateTag(new UpdatePetDto
            {
                Id = Guid.NewGuid(),
                PetTypeId = type.Id
            });

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public async Task UpdateTag_WhenNotOwner_ReturnsBadRequest()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var ownerId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var type = new PetType { Name = "t", ImagePath = "" };
        ctx.PetTypes.Add(type);
        var health = new PetHealth
        {
            Age = 2,
            PetGender = Gender.Women,
            Weight = 5f,
            Birthday = DateOnly.MinValue,
            EnergyRating = 1f,
            FriendlyRating = 1f,
            ObedienceRating = 1f,
            HealthRating = 1f,
            Description = "h"
        };
        var pet = new Pet
        {
            UserId = ownerId,
            Name = "p",
            Description = "d",
            City = "c",
            Breed = "b",
            ImagePath = "",
            Tags = new List<Tag>(),
            PetType = type,
            PetHealth = health
        };
        ctx.Pets.Add(pet);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx, out var root);
        try
        {
            sut.SetUser(otherId);
            var result = await sut.UpdateTag(new UpdatePetDto { Id = pet.Id, PetTypeId = type.Id });
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public async Task DeletePet_WhenMissing_ReturnsNotFound()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var sut = CreateSut(ctx, out var root);
        try
        {
            sut.SetUser(Guid.NewGuid());
            var result = await sut.DeletePet(new DeletePetDto { Id = Guid.NewGuid() });
            Assert.IsType<NotFoundObjectResult>(result);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public async Task GetScroll_ReturnsFilteredPets()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var viewerId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        ctx.Users.Add(new User
        {
            Id = viewerId,
            Email = $"{viewerId:N}@scroll.test",
            PasswordHash = "x",
            Name = "viewer",
            Role = "ActiveUser",
            SubscriptionTier = "Free",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        ctx.Users.Add(new User
        {
            Id = otherId,
            Email = $"{otherId:N}@scroll.test",
            PasswordHash = "x",
            Name = "owner",
            Role = "ActiveUser",
            SubscriptionTier = "Free",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        var petType = new PetType { Name = "dog", ImagePath = "" };
        ctx.PetTypes.Add(petType);
        var q = new Questionary
        {
            UserId = viewerId,
            ImagePath = "",
            Name = "viewer",
            PhoneNumber = "+10000000000",
            UserGender = Gender.Women,
            Age = 30,
            Bio = "b",
            City = "city",
            AllergicToPets = new List<PetType>(),
            WantToPets = new List<PetType>(),
            PetGender = Gender.Women,
            ageFrom = 0,
            ageTo = 20
        };
        ctx.Questionarys.Add(q);
        var health = new PetHealth
        {
            Age = 5,
            PetGender = Gender.Women,
            Weight = 4f,
            Birthday = DateOnly.MinValue,
            EnergyRating = 1f,
            FriendlyRating = 1f,
            ObedienceRating = 1f,
            HealthRating = 1f,
            Description = "h"
        };
        var pet = new Pet
        {
            UserId = otherId,
            Name = "adopt",
            Description = "d",
            City = "c",
            Breed = "b",
            ImagePath = "",
            Tags = new List<Tag>(),
            PetType = petType,
            PetHealth = health
        };
        ctx.Pets.Add(pet);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx, out var root);
        try
        {
            sut.SetUser(viewerId);
            var result = await sut.GetScroll();
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<Pet>>(ok.Value);
            Assert.Contains(list, p => p.Id == pet.Id);
        }
        finally
        {
            TryDelete(root);
        }
    }
}
