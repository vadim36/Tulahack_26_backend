using System.Security.Claims;
using backend.Data;
using backend.Services.Image;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;

namespace backend.Tests.Unit;

internal static class ControllerTestHelper
{
    public static AppDbContext NewDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    public static void SetUser(this ControllerBase controller, Guid userId, string role = "ActiveUser")
    {
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            },
            authenticationType: "Test");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    public static IConfiguration TestJwtConfiguration { get; } = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = new string('k', 64),
            ["Jwt:Issuer"] = "https://unit.tests/",
            ["Jwt:Audience"] = "https://unit.tests/",
            ["Jwt:ExpireMinutes"] = "30"
        })
        .Build();

    public static (ImageService Service, string TempRoot) CreateImageService()
    {
        var root = Path.Combine(Path.GetTempPath(), "dm_unit_img_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        var env = new Mock<IWebHostEnvironment>();
        env.Setup(e => e.WebRootPath).Returns(root);

        return (new ImageService(env.Object), root);
    }

    public static IFormFile CreateFakeJpegFormFile(int sizeBytes = 512)
    {
        var bytes = new byte[sizeBytes];
        Array.Fill(bytes, (byte)1);
        var stream = new MemoryStream(bytes);

        var mock = new Mock<IFormFile>();
        mock.Setup(f => f.FileName).Returns("unit.jpg");
        mock.Setup(f => f.Length).Returns(sizeBytes);
        mock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns<Stream, CancellationToken>((target, _) =>
            {
                stream.Position = 0;
                return stream.CopyToAsync(target);
            });

        return mock.Object;
    }
}
