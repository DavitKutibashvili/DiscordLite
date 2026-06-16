namespace DiscordLite_WEB.Services.IServices
{
    public interface IUserService
    {
        public Task<T?> GetUserProfileAsync<T>();
        public Task<T?> UpdateDisplayNameAsync<T>(string newDisplayName);
        public Task<T?> UpdateUserAvatarAsync<T>(IFormFile formFile);
        public Task<T?> DeleteUserAvatarAsync<T>();
    }
}
