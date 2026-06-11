namespace DiscordLite_WEB.Services.IServices
{
    public interface IDMChatService
    {
        Task<T> GetChatsAsync<T>();
        Task<T> GetChatAsync<T>(int chatId);
        Task<T> GetChatMessagesAsync<T>(int chatId, int page, int pageSize);
    }
}
