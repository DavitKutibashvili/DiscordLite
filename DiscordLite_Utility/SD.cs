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
            DELETE
        }
        public const string SessionAccessToken = "JWTToken";
        public const string SessionRefreshToken = "RefreshToken";
        public const string CurrentAPIVersion = "v1";
        public static string BaseAPIUrl { get; set; }
    }
}
