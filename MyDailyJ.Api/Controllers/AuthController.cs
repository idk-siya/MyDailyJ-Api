using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyDailyJ.Api.Data;
using MyDailyJ.Api.Dtos;
using MyDailyJ.Api.Models;
using MyDailyJ.Api.Services;

namespace MyDailyJ.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtTokenService _tokenService;

    public AuthController(AppDbContext db, JwtTokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var emailTaken = await _db.Users.AnyAsync(u => u.Email == email);
        if (emailTaken)
        {
            return Ok(new AuthResponse
            {
                Success = false,
                Message = "An account with that email already exists"
            });
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _tokenService.CreateToken(user);

        return Ok(new AuthResponse
        {
            Success = true,
            Message = "Account created successfully",
            Token = token
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Ok(new AuthResponse
            {
                Success = false,
                Message = "Invalid email or password"
            });
        }

        var token = _tokenService.CreateToken(user);

        return Ok(new AuthResponse
        {
            Success = true,
            Message = "Login successful",
            Token = token
        });
    }
}
