using Microsoft.AspNetCore.Http;

namespace CommunityCenter.Extensions
{
    public static class ImageExtensions
    {
        public static async Task<string?> SaveImageAsync(this IFormFile file, IWebHostEnvironment environment)
        {
            if (file == null || file.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(environment.WebRootPath, "images", "desserts");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/images/desserts/{uniqueFileName}";
        }
    }
}
