using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace backend.Tests.Support;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Frontend"] = "http://localhost:3000",
                ["Jwt:Key"] = new string('t', 64),
                ["Jwt:Issuer"] = "https://tests.local/",
                ["Jwt:Audience"] = "https://tests.local/",
                ["Jwt:ExpireMinutes"] = "60",
                ["ConnectionStrings:postgre"] = "Host=localhost;Database=unused"
            });
        });
    }
}
