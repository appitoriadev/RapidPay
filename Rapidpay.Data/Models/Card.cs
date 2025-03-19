using System.ComponentModel.DataAnnotations;

namespace Rapidpay.Data.Models;

public class Card
{
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    public User User { get; set; }
    
    [Required]
    [StringLength(16, MinimumLength = 16)]
    public string CardNumber { get; set; }
    
    [Required]
    [StringLength(4, MinimumLength = 4)]
    public string ExpiryMonth { get; set; }
    
    [Required]
    [StringLength(4, MinimumLength = 4)]
    public string ExpiryYear { get; set; }
    
    [Required]
    [StringLength(4, MinimumLength = 3)]
    public string Cvv { get; set; }
    
    [Required]
    [StringLength(100)]
    public string CardHolderName { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedAt { get; set; }
} 