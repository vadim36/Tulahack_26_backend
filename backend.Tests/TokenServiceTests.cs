using backend.Models.Auth;
using backend.Services.Token;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace backend.Tests;

public class TokenServiceTests
{
    private static IConfiguration TestConfiguration { get; } = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = new string('k', 64),
            ["Jwt:Issuer"] = "https://issuer.tests/",
            ["Jwt:Audience"] = "https://audience.tests/",
            ["Jwt:ExpireMinutes"] = "30"
        })
        .Build();

    [Fact]
    public async Task GenerateAccessToken_ReturnsParsableJwt()
    {
        var sut = new TokenService(TestConfiguration);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Tester",
            Email = "tester@tests.local",
            PasswordHash = "irrelevant",
            Role = "User"
        };

        var token = await sut.GenerateAccessToken(user);

        Assert.False(string.IsNullOrWhiteSpace(token));
        var parts = token.Split('.');
        Assert.Equal(3, parts.Length);
    }

    [Fact]
    public async Task GenerateRefreshToken_ReturnsBase64String()
    {
        var sut = new TokenService(TestConfiguration);
        var refresh = await sut.GenerateRefreshToken();

        Assert.False(string.IsNullOrWhiteSpace(refresh));
        var bytes = Convert.FromBase64String(refresh);
        Assert.Equal(32, bytes.Length);
    }
}
