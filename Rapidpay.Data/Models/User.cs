using System.ComponentModel.DataAnnotations;
using Npgsql.TypeMapping;

namespace Rapidpay.Data.Models;

public class User : BaseEntity<int>
{
    
    [Required]
    [StringLength(100)]
    public required string Username { get; set; }
    
    [Required]
    [StringLength(100)]
    public required string Email { get; set; }
    
    [Required]
    public required string PasswordHash { get; set; }
    
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    [Required]
    public UserType UserType {get; set;}
    public virtual ICollection<Card> Cards { get; set; } = new List<Card>();
} 

public enum UserType {
    User,
    Admin
}