using DiscordLite_Utility;
using DiscordLite_WEB.Services.IServices;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DiscordLite_WEB.Services
{
    public class TokenProvider : ITokenProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public TokenProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public void ClearToken()
        {
            _httpContextAccessor.HttpContext?.Session.Remove(SD.SessionAccessToken);
            _httpContextAccessor.HttpContext?.Session.Remove(SD.SessionRefreshToken);
        }

        public ClaimsPrincipal? CreatePrincipalFromJWTToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

                var nameIdClaim = jwt.Claims.FirstOrDefault(u => u.Type == "nameid");
                if (nameIdClaim != null)
                {
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, nameIdClaim.Value));
                }

                var nameClaim = jwt.Claims.FirstOrDefault(u => u.Type == "name");
                if (nameClaim != null)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Name, nameClaim.Value));
                }

                var emailClaim = jwt.Claims.FirstOrDefault(u => u.Type == "email");
                if (emailClaim != null)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Email, emailClaim.Value));
                }

                var roleClaims = jwt.Claims.Where(u => u.Type == "role");
                foreach (var role in roleClaims)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role.Value));
                }

                var displayNameClaim = jwt.Claims.FirstOrDefault(u => u.Type == "display_name");
                if (displayNameClaim != null)
                {
                    identity.AddClaim(new Claim("display_name", displayNameClaim.Value));
                }

                return new ClaimsPrincipal(identity);

            }
            catch
            {
                return null;
            }
        }

        public string? GetAccessToken()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString(SD.SessionAccessToken);
        }
        public string? GetRefreshToken()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString(SD.SessionRefreshToken);
        }

        public void SetToken(string accessToken, string refreshToken)
        {
            _httpContextAccessor.HttpContext?.Session.SetString(SD.SessionAccessToken, accessToken);
            _httpContextAccessor.HttpContext?.Session.SetString(SD.SessionRefreshToken, refreshToken);
        }
    }
}
