namespace CommunityCenter.ViewModels;

public class PastAuctionViewModel
{
    public int DessertId { get; set; }
    public string DessertName { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string WinnerName { get; set; } = string.Empty;
    public decimal FinalPrice { get; set; }
    public DateTime EndTime { get; set; }
}
