using backend.Services.Password;
using Xunit;

namespace backend.Tests;

public class PasswordServiceTests
{
    private readonly PasswordService _sut = new();

    [Fact]
    public void HashPassword_ReturnsNonEmptyBcryptString()
    {
        var hash = _sut.HashPassword("my-secret-password");
        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.StartsWith("$2", hash, StringComparison.Ordinal);
    }

    [Fact]
    public void VerifyPassword_WithSamePlaintext_ReturnsTrue()
    {
        const string plain = "CorrectHorseBatteryStaple9";
        var hash = _sut.HashPassword(plain);

        Assert.True(_sut.VerifyPassword(hash, plain));
    }

    [Fact]
    public void VerifyPassword_WithWrongPlaintext_ReturnsFalse()
    {
        var hash = _sut.HashPassword("original");
        Assert.False(_sut.VerifyPassword(hash, "different"));
    }
}
