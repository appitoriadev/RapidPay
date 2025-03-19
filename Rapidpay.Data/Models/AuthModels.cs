namespace Rapidpay.Data.Models;

public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class RegisterRequest
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}

public class AuthResponse
{
    public string Token { get; set; }
    public string Username { get; set; }
    public DateTime ExpiresAt { get; set; }
} 