using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using backend.Models.Auth.Dto;
using backend.Models.Auth.Response;
using backend.Tests.Support;
using Xunit;

namespace backend.Tests;

public class AuthIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AuthIntegrationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetStatus_ReturnsOnline()
    {
        var response = await _client.GetAsync("/auth/status");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<StatusResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal("online", body.Status);
        Assert.False(string.IsNullOrWhiteSpace(body.Date));
    }

    [Fact]
    public async Task Register_ThenLogin_ReturnsTokens()
    {
        var email = $"user-{Guid.NewGuid():N}@test.local";
        const string password = "SecurePass123!";

        var reg = await _client.PostAsJsonAsync("/auth/register", new RegisterDto
        {
            Email = email,
            Password = password
        });
        Assert.Equal(HttpStatusCode.OK, reg.StatusCode);

        var login = await _client.PostAsJsonAsync("/auth/login", new LoginDto
        {
            Email = email,
            Password = password
        });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var tokens = await login.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);
        Assert.NotNull(tokens);
        Assert.False(string.IsNullOrWhiteSpace(tokens.accessToken));
        Assert.False(string.IsNullOrWhiteSpace(tokens.refreshToken));
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        var email = $"dup-{Guid.NewGuid():N}@test.local";
        const string password = "SecurePass123!";

        var first = await _client.PostAsJsonAsync("/auth/register", new RegisterDto
        {
            Email = email,
            Password = password
        });
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var second = await _client.PostAsJsonAsync("/auth/register", new RegisterDto
        {
            Email = email,
            Password = password
        });
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var email = $"wrong-{Guid.NewGuid():N}@test.local";
        await _client.PostAsJsonAsync("/auth/register", new RegisterDto
        {
            Email = email,
            Password = "RightPassword1!"
        });

        var login = await _client.PostAsJsonAsync("/auth/login", new LoginDto
        {
            Email = email,
            Password = "OtherPassword1!"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    [Fact]
    public async Task Login_UnknownUser_ReturnsUnauthorized()
    {
        var login = await _client.PostAsJsonAsync("/auth/login", new LoginDto
        {
            Email = $"ghost-{Guid.NewGuid():N}@test.local",
            Password = "AnyPassword1!"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    [Fact]
    public async Task GetMe_WithoutToken_ReturnsUnauthorized()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/auth/me");
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_WithBearer_ReturnsProfile()
    {
        var email = $"me-{Guid.NewGuid():N}@test.local";
        const string password = "SecurePass123!";

        await _client.PostAsJsonAsync("/auth/register", new RegisterDto { Email = email, Password = password });
        var login = await _client.PostAsJsonAsync("/auth/login", new LoginDto { Email = email, Password = password });
        var tokens = await login.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);
        Assert.NotNull(tokens);

        using var request = new HttpRequestMessage(HttpMethod.Get, "/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.accessToken);

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var profile = await response.Content.ReadFromJsonAsync<ProfileResponse>(JsonOptions);
        Assert.NotNull(profile);
        Assert.Equal(email, profile.Email);
        Assert.Equal("User", profile.Role);
        Assert.False(string.IsNullOrEmpty(profile.Id));
    }

    [Fact]
    public async Task Refresh_WithValidRefreshToken_ReturnsAccessToken()
    {
        var email = $"refresh-{Guid.NewGuid():N}@test.local";
        const string password = "SecurePass123!";

        await _client.PostAsJsonAsync("/auth/register", new RegisterDto { Email = email, Password = password });
        var login = await _client.PostAsJsonAsync("/auth/login", new LoginDto { Email = email, Password = password });
        var tokens = await login.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);
        Assert.NotNull(tokens);

        var refresh = await _client.PostAsJsonAsync("/auth/refresh", new RefreshTokenDto
        {
            RefreshToken = tokens.refreshToken
        });
        Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);

        var body = await refresh.Content.ReadFromJsonAsync<RefreshResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.accessToken));
    }

    [Fact]
    public async Task Refresh_InvalidToken_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/auth/refresh", new RefreshTokenDto
        {
            RefreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_InvalidModel_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new { email = "not-an-email", password = "" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
