using CommunityCenter.Models;
using CommunityCenter.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using CommunityCenter.wwwroot.js.signalr.Hubs;
using Microsoft.Extensions.DependencyInjection;

namespace CommunityCenter.Services;

public class AuctionService
{
    private static DateTime? _auctionEndTime;
    private static bool _isAuctionStarted;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<AuctionHub> _hubContext;

    public AuctionService(IServiceScopeFactory scopeFactory, IHubContext<AuctionHub> hubContext)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
    }

    public bool IsAuctionStarted => _isAuctionStarted;
    public DateTime? AuctionEndTime => _auctionEndTime;

    public void StartAuction()
    {
        _isAuctionStarted = true;
        _auctionEndTime = DateTime.UtcNow.AddMinutes(3);
    }

    public AuctionTimerModel GetCurrentTimer()
    {
        return new AuctionTimerModel
        {
            IsAuctionStarted = _isAuctionStarted,
            EndTime = _auctionEndTime ?? DateTime.MinValue
        };
    }

    public async Task CheckAndResetExpiredAuctions()
    {
        if (!_isAuctionStarted || !_auctionEndTime.HasValue)
            return;

        var now = DateTime.UtcNow;
        if (now >= _auctionEndTime.Value)
        {
            // Reset auction state
            _isAuctionStarted = false;
            _auctionEndTime = null;

            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();

                // Update desserts in database
                var activeDesserts = await context.Desserts
                    .Where(d => d.IsActive)
                    .ToListAsync();

                foreach (var dessert in activeDesserts)
                {
                    dessert.IsActive = false;
                    dessert.EndTime = now;
                }

                await context.SaveChangesAsync();
            }

            // Notify clients that the auction has ended
            await _hubContext.Clients.All.SendAsync("AuctionEnded");
        }
    }
}
