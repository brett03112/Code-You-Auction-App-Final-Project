using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using static CommunityCenter.Models.CommunityCenterModels;

namespace CommunityCenter.Data;

public class AuctionDbContext : IdentityDbContext<ApplicationUser>
{
    public AuctionDbContext(DbContextOptions<AuctionDbContext> options)
        : base(options)
    {
    }

    public DbSet<Dessert> Desserts { get; set; }

    public DbSet<Bid> Bids { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Dessert>()
            .Property(d => d.StartingPrice)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Dessert>()
            .Property(d => d.CurrentPrice)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Bid>()
            .Property(b => b.Amount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Dessert>()
            .HasOne(d => d.WinningBidder)
            .WithMany(u => u.WonAuctions)
            .HasForeignKey(d => d.WinningBidderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Bid>()
            .HasOne(b => b.Bidder)
            .WithMany(u => u.Bids)
            .HasForeignKey(b => b.BidderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}