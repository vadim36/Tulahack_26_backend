using backend.Data;
using backend.Models.Auth;
using backend.Models.Auth.Dto;
using backend.Models.Auth.Response;
using backend.Services.Password;
using backend.Services.Token;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;
        private readonly PasswordService _passwordService;

        public AuthController(AppDbContext context, TokenService service, PasswordService passwordService)
        {
            _context = context;
            _tokenService = service;
            _passwordService = passwordService;
        }


        [HttpGet("status")]
        [AllowAnonymous]
        public async Task<ActionResult<StatusResponse>> GetStatus()
        {
            return Ok(new StatusResponse
            {
                Status = "online",
                Date = DateTime.UtcNow.ToString()
            });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<ProfileResponse>> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id.ToString() == userId);

            return Ok(new ProfileResponse
            {
                Id = userId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (user == null || !_passwordService.VerifyPassword(user.PasswordHash, dto.Password))
                return Unauthorized(new
                {
                    error = "Неверный логин или пароль"
                });

            var accessToken = await _tokenService.GenerateAccessToken(user);
            var refreshToken = await _tokenService.GenerateRefreshToken();

            _context.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken.ToString(),
                UserId = user.Id,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            await _context.SaveChangesAsync();

            return Ok(new LoginResponse
            {
                accessToken = accessToken,
                refreshToken = refreshToken
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var email = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (email != null)
                return BadRequest(new
                {
                    error = "Аккаунт с таким email уже существует!"
                });

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = _passwordService.HashPassword(dto.Password),
                Role = "User",
                Name = dto.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<RefreshResponse>> Refresh(RefreshTokenDto dto)
        {
            var token = await _context.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == dto.RefreshToken);

            if (token == null || token.IsRevoked || token.Expires < DateTime.UtcNow)
                return Unauthorized();

            var newAccessToken = await _tokenService.GenerateAccessToken(token.User);

            return Ok(new RefreshResponse { accessToken = newAccessToken });
        }
        
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(RefreshTokenDto dto)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == dto.RefreshToken);

            if (token != null)
            {
                token.IsRevoked = true;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }
    }
}
