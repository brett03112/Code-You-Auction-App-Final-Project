using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CommunityCenter.Data;
using CommunityCenter.Services;
using CommunityCenter.wwwroot.js.signalr.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using static CommunityCenter.Models.CommunityCenterModels;

[Authorize]
public class AuctionController : Controller
{
    private readonly AuctionDbContext _context;
    private readonly IHubContext<AuctionHub> _hubContext;
    private readonly AuctionService _auctionService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuctionController(
        AuctionDbContext context, 
        IHubContext<AuctionHub> hubContext,
        AuctionService auctionService,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _hubContext = hubContext;
        _auctionService = auctionService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var activeDesserts = await _context.Desserts
            .Include(d => d.WinningBidder)
            .Where(d => d.IsActive)
            .OrderBy(d => d.EndTime)
            .ToListAsync();

        ViewBag.AuctionTimer = _auctionService.GetCurrentTimer();
        return View(activeDesserts);
    }

    [HttpPost]
    public async Task<IActionResult> PlaceBid(int dessertId, decimal bidAmount)
    {
        if (!_auctionService.IsAuctionStarted)
        {
            return Json(new { success = false, message = "Auction has not started yet" });
        }

        var dessert = await _context.Desserts.FindAsync(dessertId);
        var bidder = await _userManager.FindByNameAsync(User.Identity?.Name);

        // Validate dessert and bidder
        if (dessert == null || bidder == null)
        {
            return Json(new { success = false, message = "Invalid bid: Dessert or bidder not found" });
        }

        // Check if auction is still active
        if (!dessert.IsActive || DateTime.UtcNow >= _auctionService.AuctionEndTime)
        {
            return Json(new { success = false, message = "This auction has ended" });
        }

        // Validate bid amount is a whole dollar amount
        if (bidAmount % 1 != 0)
        {
            return Json(new { success = false, message = "Bid must be a whole dollar amount" });
        }

        // Validate bid amount is higher than current price
        if (bidAmount <= dessert.CurrentPrice)
        {
            return Json(new { success = false, message = $"Bid must be higher than ${dessert.CurrentPrice}" });
        }

        var bid = new Bid
        {
            DessertId = dessertId,
            BidderId = bidder.Id,
            Amount = bidAmount,
            TimeStamp = DateTime.UtcNow,
            Dessert = dessert,
            Bidder = bidder
        };

        dessert.CurrentPrice = bidAmount;
        dessert.WinningBidderId = bidder.Id;
        dessert.WinningBidder = bidder;

        _context.Bids.Add(bid);
        await _context.SaveChangesAsync();

        // Notify all clients of the new bid
        await _hubContext.Clients.All.SendAsync("BidUpdated",
            dessertId,
            bidAmount,
            bidder.Id,
            bidder.UserName);

        return Json(new { success = true });
    }
}
