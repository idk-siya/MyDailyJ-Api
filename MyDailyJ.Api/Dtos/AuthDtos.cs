using System.ComponentModel.DataAnnotations;

namespace MyDailyJ.Api.Dtos;

// Mirrors com.example.poefn.model.LoginRequest in the Android app.
public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

// Mirrors com.example.poefn.model.RegisterRequest in the Android app.
public class RegisterRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;
}

// Mirrors com.example.poefn.model.AuthResponse in the Android app.
public class AuthResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; }
}
