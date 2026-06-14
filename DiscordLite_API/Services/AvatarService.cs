using DiscordLite_API.Services.IServices;

namespace DiscordLite_API.Services
{
    public class AvatarService : IAvatarService
    {
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
        private readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };
        private readonly IWebHostEnvironment _webHostEnvironment;
        public AvatarService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                {
                    return false;
                }
                var fileName = Path.GetFileName(imageUrl);
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "avatars", fileName);
                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<string?> UploadImageAsync(IFormFile file)
        {
            try
            {
                if (!ValidateImage(file))
                    return null;

                var webRoot = _webHostEnvironment.WebRootPath ?? _webHostEnvironment.ContentRootPath;
                var uploadsFolder = Path.Combine(webRoot, "images", "avatars");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                return $"images/avatars/{uniqueFileName}";
            }
            catch
            {
                return null;
            }
        }

        public bool ValidateImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }
            var allowedMimeTypes = new[] { "image/jpeg", "image/png" };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                return false;

            if (file.Length > MaxFileSize)
            {
                return false;
            }
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return false;
            }
            return true;
        }
    }
}
