namespace DiscordLite_API.Services.IServices
{
    public interface IAvatarService
    {
        Task<string?> UploadImageAsync(IFormFile file);
        Task<bool> DeleteImageAsync(string imageUrl);
        bool ValidateImage(IFormFile file);
    }
}
