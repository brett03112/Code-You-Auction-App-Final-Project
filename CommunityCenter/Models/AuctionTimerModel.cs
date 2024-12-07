namespace CommunityCenter.Models;

public class AuctionTimerModel
{
    public int Days { get; set; }
    public int Hours { get; set; }
    public int Minutes { get; set; }
    
    public DateTime GetEndTime()
    {
        return DateTime.UtcNow.AddDays(Days).AddHours(Hours).AddMinutes(Minutes);
    }
}