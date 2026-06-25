using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordLite_Utility
{
    public class SD
    {
        public enum ApiType
        {
            GET,
            POST,
            PUT,
            PATCH,
            DELETE
        }
        public enum ChannelType
        {
            Text,
            Voice
        }
        public const string SessionAccessToken = "JWTToken";
        public const string SessionRefreshToken = "RefreshToken";
        public const string CurrentAPIVersion = "v1";
        public static string BaseAPIUrl { get; set; }
        public static string GetImageUrl(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return "/images/placeholder-avatar.png";
            }
            if (imageUrl.StartsWith("http"))
            {
                return imageUrl;
            }
            return $"{BaseAPIUrl}/{imageUrl}";
        }
    }
}
