using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CommunityCenter.Models;
using CommunityCenter.Data;
using CommunityCenter.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using CommunityCenter.wwwroot.js.signalr.Hubs;
using static CommunityCenter.Models.CommunityCenterModels;

namespace CommunityCenter.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AuctionDbContext _context;
    private readonly AuctionService _auctionService;
    private readonly IHubContext<AuctionHub> _hubContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AutoBidService _autoBidService;

    public HomeController(
        ILogger<HomeController> logger, 
        AuctionDbContext context, 
        AuctionService auctionService,
        IHubContext<AuctionHub> hubContext,
        UserManager<ApplicationUser> userManager,
        AutoBidService autoBidService)
    {
        _logger = logger;
        _context = context;
        _auctionService = auctionService;
        _hubContext = hubContext;
        _userManager = userManager;
        _autoBidService = autoBidService;
    }

    public IActionResult Index()
    {
        ViewBag.HasActiveAuctions = _context.Desserts.Any(d => d.IsActive);
        var timer = _auctionService.GetCurrentTimer();
        return View(timer);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> StartAuction()
    {
        _auctionService.StartAuction();
        var endTime = _auctionService.AuctionEndTime.Value;

        var desserts = await _context.Desserts.ToListAsync();
        foreach (var dessert in desserts)
        {
            dessert.IsActive = true;
            dessert.EndTime = endTime;
        }

        await _context.SaveChangesAsync();

        // Start automated bidding for test users
        _autoBidService.StartAutoBidding(endTime);

        // Notify all clients that the auction has started
        await _hubContext.Clients.All.SendAsync("AuctionStarted", endTime.ToString("o"));

        return Json(new { success = true, endTime = endTime });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
