using CommunityCenter.Data;
using CommunityCenter.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CommunityCenter.wwwroot.js.signalr.Hubs;
using static CommunityCenter.Models.CommunityCenterModels;

namespace CommunityCenter.Services;

public class AutoBidService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<AuctionHub> _hubContext;
    private readonly ILogger<AutoBidService> _logger;
    private readonly Random _random;
    private Timer? _bidTimer;
    private readonly string[] _testUsers = { "test1", "test2", "test3", "test4" };

    public AutoBidService(
        IServiceScopeFactory scopeFactory,
        IHubContext<AuctionHub> hubContext,
        ILogger<AutoBidService> logger)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _logger = logger;
        _random = new Random();
    }

    public void StartAutoBidding(DateTime endTime)
    {
        _logger.LogInformation("Starting auto-bidding until {EndTime}", endTime);

        // Stop any existing timer
        StopAutoBidding();

        // Create a new timer that triggers every 3-5 seconds
        _bidTimer = new Timer(async _ => await PlaceAutomaticBids(), null, 
            TimeSpan.FromSeconds(2), // Initial delay
            TimeSpan.FromSeconds(3)); // Interval

        _logger.LogInformation("Auto-bidding timer started");
    }

    private async Task PlaceAutomaticBids()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            _logger.LogInformation("Attempting to place automatic bids");

            var activeDesserts = await context.Desserts
                .Where(d => d.IsActive && d.EndTime > DateTime.UtcNow)
                .ToListAsync();

            _logger.LogInformation("Found {Count} active desserts", activeDesserts.Count);

            if (!activeDesserts.Any())
            {
                _logger.LogInformation("No active desserts found, stopping auto-bidding");
                StopAutoBidding();
                return;
            }

            // Each test user attempts to bid on a random dessert
            foreach (var username in _testUsers)
            {
                var bidder = await userManager.FindByNameAsync(username);
                if (bidder == null)
                {
                    _logger.LogWarning("Test user {Username} not found", username);
                    continue;
                }

                // Select a random dessert
                var dessert = activeDesserts[_random.Next(activeDesserts.Count)];

                // Calculate bid amount (2-10 dollars more than current price)
                var bidIncrease = _random.Next(2, 11);
                var bidAmount = dessert.CurrentPrice + bidIncrease;

                _logger.LogInformation("User {Username} attempting to bid ${Amount} on {DessertName}", 
                    username, bidAmount, dessert.Name);

                // Create and save the bid
                var bid = new Bid
                {
                    DessertId = dessert.Id,
                    BidderId = bidder.Id,
                    Amount = bidAmount,
                    TimeStamp = DateTime.UtcNow,
                    Dessert = dessert,
                    Bidder = bidder
                };

                dessert.CurrentPrice = bidAmount;
                dessert.WinningBidderId = bidder.Id;
                dessert.WinningBidder = bidder;

                context.Bids.Add(bid);
                await context.SaveChangesAsync();

                _logger.LogInformation("Bid placed successfully by {Username}", username);

                // Broadcast the bid update to all clients
                await _hubContext.Clients.All.SendAsync("BidUpdated", 
                    dessert.Id,
                    bidAmount,
                    bidder.Id,
                    bidder.UserName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in auto-bidding");
        }
    }

    public void StopAutoBidding()
    {
        if (_bidTimer != null)
        {
            _bidTimer.Dispose();
            _bidTimer = null;
            _logger.LogInformation("Auto-bidding stopped");
        }
    }
}
