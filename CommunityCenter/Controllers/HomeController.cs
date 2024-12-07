using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CommunityCenter.Models;
using CommunityCenter.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CommunityCenter.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AuctionDbContext _context;

    public HomeController(ILogger<HomeController> logger, AuctionDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        ViewBag.HasActiveAuctions = _context.Desserts.Any(d => d.IsActive);
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> StartAuction([FromBody] AuctionTimerModel timer)
    {
        // Enforce 2-minute maximum for testing
        var totalMinutes = (timer.Days * 24 * 60) + (timer.Hours * 60) + timer.Minutes;
        if (totalMinutes > 2)
        {
            timer.Days = 0;
            timer.Hours = 0;
            timer.Minutes = 2;
        }

        var endTime = timer.GetEndTime();
        var desserts = await _context.Desserts.ToListAsync();
        
        foreach (var dessert in desserts)
        {
            dessert.IsActive = true;
            dessert.EndTime = endTime;
        }

        await _context.SaveChangesAsync();
        return Json(new { success = true });
    }
}
