using Microsoft.EntityFrameworkCore;
using Rapidpay.Data.Models;

namespace Rapidpay.Data;

public class RapidpayDbContext : DbContext
{
    public RapidpayDbContext(DbContextOptions<RapidpayDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Card> Cards { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<OAuthClient> OAuthClients { get; set; }
    public DbSet<OAuthToken> OAuthTokens { get; set; }
    public DbSet<OAuthAuthorizationCode> OAuthAuthorizationCodes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Card>()
            .HasIndex(c => c.CardNumber)
            .IsUnique();

        modelBuilder.Entity<Card>()
            .HasOne(c => c.User)
            .WithMany(u => u.Cards)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Card)
            .WithMany()
            .HasForeignKey(t => t.CardId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OAuthClient>()
            .HasIndex(c => c.ClientId)
            .IsUnique();

        modelBuilder.Entity<OAuthAuthorizationCode>()
            .HasIndex(c => c.Code)
            .IsUnique();

        modelBuilder.Entity<OAuthToken>()
            .HasIndex(t => t.AccessToken)
            .IsUnique();

        modelBuilder.Entity<OAuthToken>()
            .HasIndex(t => t.RefreshToken)
            .IsUnique();
    }
} 