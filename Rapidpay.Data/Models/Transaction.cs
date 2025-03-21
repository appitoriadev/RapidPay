using System.ComponentModel.DataAnnotations;

namespace Rapidpay.Data.Models;

public class Transaction : BaseEntity<int>
{
    [Required]
    public required Card Card { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
    
    [Required]
    [StringLength(3, MinimumLength = 3)]
    public required string Currency { get; set; }
    
    [Required]
    public required TransactionStatus Status { get; set; }
    
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}

public enum TransactionStatus
{
    Pending,
    Completed,
    Failed,
    Cancelled
} 