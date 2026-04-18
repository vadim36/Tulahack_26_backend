using backend.Controllers;
using backend.Data;
using backend.Models.Auth;
using backend.Models.Auth.Dto;
using backend.Models.Auth.Response;
using backend.Services.Password;
using backend.Services.Token;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests.Unit;

public class AuthControllerUnitTests
{
    private static AuthController CreateSut(AppDbContext ctx)
    {
        var tokens = new TokenService(ControllerTestHelper.TestJwtConfiguration);
        var passwords = new PasswordService();
        return new AuthController(ctx, tokens, passwords);
    }

    [Fact]
    public async Task GetStatus_ReturnsOnlinePayload()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var sut = CreateSut(ctx);

        var result = await sut.GetStatus();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<StatusResponse>(ok.Value);
        Assert.Equal("online", body.Status);
    }

    [Fact]
    public async Task Register_NewEmail_ReturnsOk()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var sut = CreateSut(ctx);

        var result = await sut.Register(new RegisterDto
        {
            Email = "new@unit.test",
            Password = "Secret123!"
        });

        Assert.IsType<OkResult>(result);
        Assert.Equal(1, await ctx.Users.CountAsync());
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var sut = CreateSut(ctx);
        var dto = new RegisterDto { Email = "dup@unit.test", Password = "Secret123!" };

        await sut.Register(dto);
        var result = await sut.Register(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokens()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var passwords = new PasswordService();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "login@unit.test",
            PasswordHash = passwords.HashPassword("Secret123!"),
            Name = "login@unit.test",
            Role = "User",
            SubscriptionTier = "Free",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx);
        var result = await sut.Login(new LoginDto { Email = user.Email, Password = "Secret123!" });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<LoginResponse>(ok.Value);
        Assert.False(string.IsNullOrEmpty(body.accessToken));
        Assert.False(string.IsNullOrEmpty(body.refreshToken));
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var passwords = new PasswordService();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "bad@unit.test",
            PasswordHash = passwords.HashPassword("Right1!"),
            Name = "bad@unit.test",
            Role = "User",
            SubscriptionTier = "Free",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx);
        var result = await sut.Login(new LoginDto { Email = user.Email, Password = "Wrong1!" });

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetProfile_WithClaims_ReturnsProfile()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "me@unit.test",
            PasswordHash = "x",
            Name = "Me",
            Role = "ActiveUser",
            SubscriptionTier = "Free",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx);
        sut.SetUser(user.Id, "ActiveUser");

        var result = await sut.GetProfile();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<ProfileResponse>(ok.Value);
        Assert.Equal(user.Email, body.Email);
        Assert.Equal(user.Id.ToString(), body.Id);
    }

    [Fact]
    public async Task Refresh_ValidToken_ReturnsAccessToken()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "r@unit.test",
            PasswordHash = "x",
            Name = "r",
            Role = "User",
            SubscriptionTier = "Free",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        ctx.Users.Add(user);
        var rt = new RefreshToken
        {
            Token = "refresh-token-unit",
            UserId = user.Id,
            User = user,
            Expires = DateTime.UtcNow.AddDays(1),
            IsRevoked = false
        };
        ctx.RefreshTokens.Add(rt);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx);
        var result = await sut.Refresh(new RefreshTokenDto { RefreshToken = rt.Token });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<RefreshResponse>(ok.Value);
        Assert.False(string.IsNullOrEmpty(body.accessToken));
    }

    [Fact]
    public async Task Logout_ExistingToken_RevokesAndReturnsOk()
    {
        await using var ctx = ControllerTestHelper.NewDbContext();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "out@unit.test",
            PasswordHash = "x",
            Name = "out",
            Role = "User",
            SubscriptionTier = "Free",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        ctx.Users.Add(user);
        var rt = new RefreshToken
        {
            Token = "logout-token",
            UserId = user.Id,
            Expires = DateTime.UtcNow.AddDays(1),
            IsRevoked = false
        };
        ctx.RefreshTokens.Add(rt);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx);
        var result = await sut.Logout(new RefreshTokenDto { RefreshToken = rt.Token });

        Assert.IsType<OkResult>(result);
        await ctx.Entry(rt).ReloadAsync();
        Assert.True(rt.IsRevoked);
    }
}
