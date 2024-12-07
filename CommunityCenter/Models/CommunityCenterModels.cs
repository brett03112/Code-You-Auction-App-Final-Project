using Microsoft.AspNetCore.Identity;

namespace CommunityCenter.Models;

public class CommunityCenterModels
{
    public class Dessert
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public string? ImageUrl { get; set; }
        public decimal StartingPrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsActive { get; set; }
        public string? WinningBidderId { get; set; }
        public virtual ApplicationUser? WinningBidder { get; set; }
        public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();
    }

    public class Bid
    {
        public int Id { get; set; }
        public int DessertId { get; set; }
        public required string BidderId { get; set; }
        public decimal Amount { get; set; }
        public DateTime TimeStamp { get; set; }
        public virtual required Dessert Dessert { get; set; }
        public virtual required ApplicationUser Bidder { get; set; }
    }

    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();
        public virtual ICollection<Dessert> WonAuctions { get; set; } = new List<Dessert>();
    }
}
