namespace CommunityCenter.Models;

public class AuctionTimerModel
{
    public DateTime EndTime { get; set; }
    public bool IsAuctionStarted { get; set; }
    
    public AuctionTimerModel()
    {
        // Initialize with no end time and auction not started
        IsAuctionStarted = false;
        EndTime = DateTime.MinValue;
    }

    public DateTime GetEndTime()
    {
        // Always return 3 minutes from now
        return DateTime.UtcNow.AddMinutes(3);
    }
    
    public bool IsExpired()
    {
        return DateTime.UtcNow >= EndTime;
    }
    
    public TimeSpan GetRemainingTime()
    {
        var remaining = EndTime - DateTime.UtcNow;
        return remaining.TotalMilliseconds > 0 ? remaining : TimeSpan.Zero;
    }
}
