using Microsoft.AspNetCore.Identity;
using static CommunityCenter.Models.CommunityCenterModels;

namespace CommunityCenter.Services;

public class UserSeederService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserSeederService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedTestUsers()
    {
        // Create test users if they don't exist
        for (int i = 1; i <= 4; i++)
        {
            string username = $"test{i}";
            string email = $"test{i}@example.com";
            string password = "Test123!";

            if (await _userManager.FindByNameAsync(username) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = username,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create test user {username}: {string.Join(", ", result.Errors)}");
                }
            }
        }
    }
}
