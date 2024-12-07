using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CommunityCenter.Data;
using CommunityCenter.wwwroot.js.signalr.Hubs;
using Microsoft.AspNetCore.SignalR;
using static CommunityCenter.Models.CommunityCenterModels;
using static CommunityCenter.wwwroot.js.signalr.Hubs.AuctionHub;
using static CommunityCenter.Extensions.ImageExtensions;
using CommunityCenter.Extensions;

[Authorize]
public class AuctionController : Controller
{
    private readonly AuctionDbContext _context;
    private readonly IHubContext<AuctionHub> _hubContext;
    private readonly IWebHostEnvironment _environment;

    public AuctionController(AuctionDbContext context, IHubContext<AuctionHub> hubContext, IWebHostEnvironment environment)
    {
        _context = context;
        _hubContext = hubContext;
        _environment = environment;
    }

    public async Task<IActionResult> Index()
    {
        var activeDesserts = await _context.Desserts
            .Include(d => d.WinningBidder)
            .Where(d => d.IsActive)
            .OrderBy(d => d.EndTime)
            .ToListAsync();
        return View(activeDesserts);
    }

    [HttpPost]
    public async Task<IActionResult> PlaceBid(int dessertId, decimal bidAmount)
    {
        var dessert = await _context.Desserts.FindAsync(dessertId);
        var bidder = await _context.Users.FindAsync(User.Identity?.Name);

        if (dessert == null || !dessert.IsActive || bidAmount <= dessert.CurrentPrice || bidder == null)
        {
            return Json(new { success = false, message = "Invalid bid" });
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

        _context.Bids.Add(bid);
        await _context.SaveChangesAsync();

        await _hubContext.Clients.All.SendAsync("BidUpdated",
            dessertId,
            bidAmount,
            bidder.UserName,
            bidder.UserName);

        return Json(new { success = true });
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Dessert dessert, IFormFile? image = null)
    {
        if (ModelState.IsValid)
        {
            if (image != null)
            {
                dessert.ImageUrl = await image.SaveImageAsync(_environment);
            }
        
            dessert.IsActive = true;
            dessert.CurrentPrice = dessert.StartingPrice;
            _context.Add(dessert);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(dessert);
    }
}
