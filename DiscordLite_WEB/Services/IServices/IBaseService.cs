using DiscordLite_DTO;
using DiscordLite_WEB.Models;
namespace DiscordLite_WEB.Services.IServices
{
    public interface IBaseService
    {
        ApiResponse<object> ResponseModel { get; set; }
        Task<T?> SendAsync<T>(ApiRequest apiRequest, bool withBearer = true);
    }
}
