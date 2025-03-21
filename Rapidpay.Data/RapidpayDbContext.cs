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
    // public DbSet<OAuthClient> OAuthClients { get; set; }
    // public DbSet<OAuthToken> OAuthTokens { get; set; }
    // public DbSet<OAuthAuthorizationCode> OAuthAuthorizationCodes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if(modelBuilder == null)
        {
            return;
        }

        modelBuilder.Entity<User>().ToTable("User").HasKey(k => k.Id);
        modelBuilder.Entity<Card>().ToTable("Card").HasKey(k => k.Id);
        modelBuilder.Entity<Transaction>().ToTable("Transaction").HasKey(k => k.Id);

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
            .HasForeignKey(c => c.Id)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Card)
            .WithMany()
            .HasForeignKey(t => t.Id)
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);

        // modelBuilder.Entity<OAuthClient>()
        //     .HasIndex(c => c.ClientId)
        //     .IsUnique();

        // modelBuilder.Entity<OAuthAuthorizationCode>()
        //     .HasIndex(c => c.Code)
        //     .IsUnique();

        // modelBuilder.Entity<OAuthToken>()
        //     .HasIndex(t => t.AccessToken)
        //     .IsUnique();

        // modelBuilder.Entity<OAuthToken>()
        //     .HasIndex(t => t.RefreshToken)
        //     .IsUnique();
    }
} 