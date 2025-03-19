using System.ComponentModel.DataAnnotations;

namespace Rapidpay.Data.Models;

public class User
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Username { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Email { get; set; }
    
    [Required]
    public string PasswordHash { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    
    public virtual ICollection<Card> Cards { get; set; } = new List<Card>();
} 