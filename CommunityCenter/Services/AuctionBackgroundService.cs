using Microsoft.Extensions.Hosting;
using CommunityCenter.Services;

namespace CommunityCenter.Services;

public class AuctionBackgroundService : BackgroundService
{
    private readonly AuctionService _auctionService;

    public AuctionBackgroundService(AuctionService auctionService)
    {
        _auctionService = auctionService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _auctionService.CheckAndResetExpiredAuctions();
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
