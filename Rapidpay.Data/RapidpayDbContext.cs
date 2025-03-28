using Microsoft.EntityFrameworkCore;
using Rapidpay.Data.Models;

namespace Rapidpay.Data;

public class RapidpayDbContext : DbContext
{
    public RapidpayDbContext(DbContextOptions<RapidpayDbContext> options)
        : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Card> Cards { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

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
            .HasOne<User>()
            .WithMany(u => u.Cards)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Transaction>()
            .HasOne<Card>()
            .WithMany()
            .HasForeignKey(t => t.CardId)
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);
    }
} 