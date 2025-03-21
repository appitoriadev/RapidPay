using System.ComponentModel.DataAnnotations;
namespace Rapidpay.Data.Models;

public class LoginRequest: BaseEntity<int>
{
    [Required]
    public required string Username { get; set; }
    [Required]
    public required string Password { get; set; }
}

public class AuthResponse: BaseEntity<int>
{
    [Required]
    public required string Token { get; set; }
    [Required]
    public required string RefreshToken { get; set; }
    [Required]
    public required string Username { get; set; }
    [Required]
    public required DateTime ExpiresAt { get; set; }
} 