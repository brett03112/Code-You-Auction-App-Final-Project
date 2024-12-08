using CommunityCenter.Models;

namespace CommunityCenter.Services;

public class AuctionService
{
    private static DateTime? _auctionEndTime;
    private static bool _isAuctionStarted;

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
}
