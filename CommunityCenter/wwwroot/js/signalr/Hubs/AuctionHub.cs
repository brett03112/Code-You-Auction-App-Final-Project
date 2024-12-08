using Microsoft.AspNetCore.SignalR;

namespace CommunityCenter.wwwroot.js.signalr.Hubs;

public class AuctionHub : Hub
{
    public async Task UpdateBid(int dessertId, decimal newPrice, string bidderId, string bidderName)
    {
        await Clients.All.SendAsync("BidUpdated", dessertId, newPrice, bidderId, bidderName);
    }

    public async Task StartAuction(string endTime)
    {
        await Clients.All.SendAsync("AuctionStarted", endTime);
    }
}
