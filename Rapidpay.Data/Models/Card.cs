using System.ComponentModel.DataAnnotations;

namespace Rapidpay.Data.Models;

public class Card: BaseEntity<int>
{
    [Required]
    public required User User { get; set; }
    
    [Required]
    [StringLength(16, MinimumLength = 16)]
    public required string CardNumber { get; set; }
    
    [Required]
    [StringLength(4, MinimumLength = 4)]
    public required string ExpiryMonth { get; set; }
    
    [Required]
    [StringLength(4, MinimumLength = 4)]
    public required string ExpiryYear { get; set; }
    
    [Required]
    [StringLength(4, MinimumLength = 3)]
    public required string Cvv { get; set; }
    
    [Required]
    [StringLength(100)]
    public required string CardHolderName { get; set; }
    
    [Required]
    public decimal Balance { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedAt { get; set; }
} 