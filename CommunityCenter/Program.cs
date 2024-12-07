using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CommunityCenter.Data;
using static CommunityCenter.Models.CommunityCenterModels;
using CommunityCenter.wwwroot.js.signalr.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

builder.Services.AddDbContext<AuctionDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
        options.SignIn.RequireConfirmedAccount = false; // Changed to false
        options.Password.RequireDigit = false;          // Simplified password requirements
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 4;            // Reduced minimum length
    })
    .AddEntityFrameworkStores<AuctionDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auction}/{action=Index}/{id?}");
app.MapHub<AuctionHub>("/auctionHub");
app.MapRazorPages();

// Seed admin role and user
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    var adminUser = await userManager.FindByEmailAsync("admin@example.com");
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = "admin@example.com",
            Email = "admin@example.com",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(adminUser, "admin00");
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
    try 
    {
        if (!context.Desserts.Any())
        {
            var desserts = new List<Dessert>
            {
                new Dessert
                {
                    Name = "Decadent Chocolate Brownies",
                    Description = "Ultra-fudgy brownies with molten chocolate centers. Made with premium dark chocolate and topped with a perfect crackly crust. Each bite reveals pools of melted chocolate for an irresistibly rich experience.",
                    StartingPrice = 25.00M,
                    CurrentPrice = 25.00M,
                    ImageUrl = "/images/desserts/brownies.jpg",
                    EndTime = DateTime.UtcNow.AddDays(7),
                    IsActive = true
                },
                new Dessert
                {
                    Name = "Holiday Snickerdoodle Cookies",
                    Description = "Classic snickerdoodle cookies with the perfect balance of cinnamon and sugar. Soft and chewy on the inside with a slight crunch on the outside. A dozen freshly baked cookies perfect for the holiday season.",
                    StartingPrice = 25.00M,
                    CurrentPrice = 25.00M,
                    ImageUrl = "/images/desserts/snickerdoodle_cookies.jpg",
                    EndTime = DateTime.UtcNow.AddDays(7),
                    IsActive = true
                },
                new Dessert
                {
                    Name = "Fresh Peach Cobbler",
                    Description = "Homestyle peach cobbler featuring juicy, caramelized peaches under a golden-brown buttery crust. Baked to perfection with a hint of cinnamon and vanilla. Perfect warm with a scoop of vanilla ice cream.",
                    StartingPrice = 25.00M,
                    CurrentPrice = 25.00M,
                    ImageUrl = "/images/desserts/peach_cobbler.jpg",
                    EndTime = DateTime.UtcNow.AddDays(7),
                    IsActive = true
                },
                new Dessert
                {
                    Name = "Classic Apple Pie",
                    Description = "Traditional lattice-topped apple pie with a buttery, flaky crust. Filled with a perfect blend of tart and sweet apples, spiced with cinnamon and nutmeg. A timeless favorite for any occasion.",
                    StartingPrice = 25.00M,
                    CurrentPrice = 25.00M,
                    ImageUrl = "/images/desserts/apple_pie.jpg",
                    EndTime = DateTime.UtcNow.AddDays(7),
                    IsActive = true
                },
                new Dessert
                {
                    Name = "Banana Pudding Delight",
                    Description = "Luxurious banana pudding layered with vanilla wafers and fresh banana slices. Topped with whipped cream, caramel drizzle, and more vanilla wafers. A perfect blend of creamy and crunchy textures.",
                    StartingPrice = 25.00M,
                    CurrentPrice = 25.00M,
                    ImageUrl = "/images/desserts/banana_pudding.jpg",
                    EndTime = DateTime.UtcNow.AddDays(7),
                    IsActive = true
                },
                new Dessert
                {
                    Name = "Orange Creamsicle Cheesecake",
                    Description = "A luxurious cheesecake topped with orange-flavored mirror glaze, decorated with fresh orange slices and whipped cream rosettes. Features a buttery graham cracker crust and rich, creamy filling infused with natural orange essence.",
                    StartingPrice = 25.00M,
                    CurrentPrice = 25.00M,
                    ImageUrl = "/images/desserts/orange_slice_cheesecake.jpg",
                    EndTime = DateTime.UtcNow.AddDays(7),
                    IsActive = true
                },
                new Dessert
                {
                    Name = "Classic Key Lime Tart",
                    Description = "A perfectly balanced key lime tart with a crisp, golden shortbread crust. Topped with delicate whipped cream dollops. The filling is silky smooth with the ideal blend of tangy and sweet, made with authentic key lime juice.",
                    StartingPrice = 25.00M,
                    CurrentPrice = 25.00M,
                    ImageUrl = "/images/desserts/key_lime_pie.jpg",
                    EndTime = DateTime.UtcNow.AddDays(7),
                    IsActive = true
                },
                new Dessert
                {
                    Name = "Rustic Rhubarb Pie",
                    Description = "A beautiful lattice-topped rhubarb pie with a flaky, hand-crimped crust. Filled with fresh, ruby-red rhubarb in a sweet-tart filling. The lattice top is golden brown and dusted with sparkling sugar.",
                    StartingPrice = 25.00M,
                    CurrentPrice = 25.00M,
                    ImageUrl = "/images/desserts/strawberry_rhubarb_pie.jpg",
                    EndTime = DateTime.UtcNow.AddDays(7),
                    IsActive = true
                },
                new Dessert
                {
                    Name = "Triple-Layer Strawberry Shortcake",
                    Description = "An elegant three-layer vanilla sponge cake filled with fresh strawberries and whipped cream. Each layer features perfectly sliced strawberries and light-as-air cream, topped with more berries and decorative cream piping.",
                    StartingPrice = 25.00M,
                    CurrentPrice = 25.00M,
                    ImageUrl = "/images/desserts/strawberry_shortcake.jpg",
                    EndTime = DateTime.UtcNow.AddDays(7),
                    IsActive = true
                },
                new Dessert
                {
                    Name = "German Chocolate Bundt Cake",
                    Description = "A rich chocolate bundt cake filled with coconut-caramel filling. Topped with shredded coconut and drizzled with caramel. The cake is perfectly moist with a tender crumb and deep chocolate flavor.",
                    StartingPrice = 25.00M,
                    CurrentPrice = 25.00M,
                    ImageUrl = "/images/desserts/German_chocolate_bundt_cake.jpg",
                    EndTime = DateTime.UtcNow.AddDays(7),
                    IsActive = true
                }
            };

            context.Desserts.AddRange(desserts);
            await context.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the desserts database.");
    }
}
app.Run();
