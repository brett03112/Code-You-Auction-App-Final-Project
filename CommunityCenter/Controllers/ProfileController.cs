using CommunityCenter.Data;
using CommunityCenter.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static CommunityCenter.Models.CommunityCenterModels;

namespace CommunityCenter.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly AuctionDbContext _context;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            AuctionDbContext context)
        {
            _userManager = userManager;
            _environment = environment;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new ProfileViewModels
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Update(ProfileViewModels model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            if (user.Email != model.Email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to update email address.");
                    return View("Index", model);
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["StatusMessage"] = "Your profile has been updated";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("Index", model);
        }

        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModels());
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModels model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.ChangePasswordAsync(user,
                model.CurrentPassword!, model.NewPassword!);

            if (result.Succeeded)
            {
                TempData["StatusMessage"] = "Your password has been changed";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        public async Task<IActionResult> BiddingHistory()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var biddingHistory = await _context.Bids
                .Include(b => b.Dessert)
                .Where(b => b.BidderId == user.Id)
                .OrderByDescending(b => b.TimeStamp)
                .ToListAsync();

            return View(biddingHistory);
        }

        public async Task<IActionResult> WonAuctions()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var wonAuctions = await _context.Desserts
                .Where(d => d.WinningBidderId == user.Id && !d.IsActive)
                .OrderByDescending(d => d.EndTime)
                .ToListAsync();

            return View(wonAuctions);
        }
    }
}
