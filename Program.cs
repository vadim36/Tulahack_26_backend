using backend.Data;
using backend.Services.Image;
using backend.Services.Password;
using backend.Services.Token;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

#region Logger

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss}][{Level:u10}] {Message:lj} {NewLine}{Exception}"
    )
    /* 
    .WriteTo.File(
        path: "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}][{Level:u10}] {Message:lj} {NewLine}{Exception}"
    )
    */
    .CreateLogger();

#endregion

builder.Services.AddControllers();

var frontend = builder.Configuration["Frontend"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        
        policy
            .WithOrigins(frontend)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

#region Services Mapping

builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<ImageService>();

#endregion

builder.Services.AddDbContext<AppDbContext>((options) => {
    var connectionStrings = builder.Configuration.GetSection("ConnectionStrings");

    options.UseNpgsql(connectionStrings["postgre"]);
});

#region Swagger

builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<AuthorizeCheckOperationFilter>();
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Scheme = "bearer",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http
                },
            new List<string>()
            }
        });
});

#endregion

#region JWT

var jwtConfiguration = builder.Configuration.GetSection("Jwt");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,

            ValidIssuer = jwtConfiguration["Issuer"],
            ValidAudience = jwtConfiguration["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration["Key"]))
        };
    });

builder.Services.AddAuthorization();

#endregion

try { 
    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            context.Database.EnsureCreated();
        }
    }

    app.UseCors("AllowFrontend");

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.UseHttpsRedirection();

    app.Run();
} catch (Exception ex) {
    Log.Fatal(ex.ToString());
} finally
{
    Log.CloseAndFlush();
}