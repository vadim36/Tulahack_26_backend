using backend.Controllers;
using backend.Data;
using backend.Models.Auth;
using backend.Models.Chats;
using backend.Models.Pets;
using backend.Models.PetsType;
using backend.Models.Questionarys;
using backend.Models.Share;
using backend.Models.Tags;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace backend.Tests.Unit;

public class ChatControllerUnitTests
{
    private static User CreateUser(Guid id, string email)
    {
        return new User
        {
            Id = id,
            Email = email,
            PasswordHash = "x",
            Name = email,
            Role = "ActiveUser",
            SubscriptionTier = "Free",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static Questionary CreateQuestionary(Guid userId)
    {
        return new Questionary
        {
            UserId = userId,
            ImagePath = "/q.png",
            Name = "Q",
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
    }

    [Fact]
    public async Task GetUserChats_ReturnsOk()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var sut = new ChatController(ctx);
        sut.SetUser(Guid.NewGuid());

        var result = await sut.GetUserChats();

        Assert.IsType<OkResult>(result.Result);
    }

    [Fact]
    public async Task GetChat_WhenUserNotParticipant_ReturnsBadRequest()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var u1 = CreateUser(Guid.NewGuid(), "a@chat.test");
        var u2 = CreateUser(Guid.NewGuid(), "b@chat.test");
        var outsider = CreateUser(Guid.NewGuid(), "c@chat.test");
        var q1 = CreateQuestionary(u1.Id);
        var q2 = CreateQuestionary(u2.Id);
        u1.Questionary = q1;
        u2.Questionary = q2;
        ctx.Users.AddRange(u1, u2, outsider);
        ctx.Questionarys.AddRange(q1, q2);

        var petType = new PetType { Name = "t", ImagePath = "" };
        ctx.PetTypes.Add(petType);
        var pet = new Pet
        {
            UserId = u1.Id,
            Name = "pet",
            Description = "d",
            City = "c",
            Breed = "b",
            ImagePath = "",
            Tags = new List<Tag>(),
            PetType = petType
        };
        ctx.Pets.Add(pet);

        var chat = new Chat
        {
            FirstUser = u1,
            SecondUser = u2,
            Pet = pet,
            Messages = new List<ChatMessage>()
        };
        ctx.Chats.Add(chat);
        await ctx.SaveChangesAsync();

        var sut = new ChatController(ctx);
        sut.SetUser(outsider.Id);

        var result = await sut.GetChat(chat.Id.ToString());

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task DeleteChat_WhenParticipant_ReturnsOk()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var u1 = CreateUser(Guid.NewGuid(), "d1@chat.test");
        var u2 = CreateUser(Guid.NewGuid(), "d2@chat.test");
        var q1 = CreateQuestionary(u1.Id);
        var q2 = CreateQuestionary(u2.Id);
        u1.Questionary = q1;
        u2.Questionary = q2;
        ctx.Users.AddRange(u1, u2);
        ctx.Questionarys.AddRange(q1, q2);

        var petType = new PetType { Name = "t", ImagePath = "" };
        ctx.PetTypes.Add(petType);
        var pet = new Pet
        {
            UserId = u1.Id,
            Name = "pet",
            Description = "d",
            City = "c",
            Breed = "b",
            ImagePath = "",
            Tags = new List<Tag>(),
            PetType = petType
        };
        ctx.Pets.Add(pet);

        var chat = new Chat { FirstUser = u1, SecondUser = u2, Pet = pet, Messages = new List<ChatMessage>() };
        ctx.Chats.Add(chat);
        await ctx.SaveChangesAsync();

        var sut = new ChatController(ctx);
        sut.SetUser(u1.Id);

        var result = await sut.DeleteChat(chat.Id.ToString());

        Assert.IsType<OkResult>(result);
        Assert.Equal(0, ctx.Chats.Count());
    }
}
