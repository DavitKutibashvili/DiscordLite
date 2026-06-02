using System.Security.AccessControl;
using static DiscordLite_Utility.SD;

namespace DiscordLite_WEB.Models
{
    public class ApiRequest
    {
        public ApiType ApiType { get; set; } = ApiType.GET;
        public string? Url { get; set; }
        public object? Data { get; set; }
        public string? Token { get; set; }
    }
}
