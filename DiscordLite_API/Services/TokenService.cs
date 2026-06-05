using DiscordLite_API.Data;
using DiscordLite_API.Model;
using DiscordLite_API.Services.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DiscordLite_API.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _db;
        public TokenService(IConfiguration configuration, UserManager<User> userManager, ApplicationDbContext db)
        {
            _configuration = configuration;
            _userManager = userManager;
            _db = db;
        }
        public async Task<string> GenerateJwtTokenAsync(User user)
        {
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JwtSettings")["SecretKey"]!);
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.UserName!),           // unique username
                new Claim("display_name", user.DisplayName ?? user.UserName!), // display name
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add all roles safely — no crash if user has none
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(9),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        public async Task<string> GenerateRefreshTokenAsync()
        {
            var randomNumber = new byte[64];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            var randomToken = Convert.ToBase64String(randomNumber);
            bool exists = _db.RefreshTokens.Any(rt => rt.RefreshTokenValue == randomToken);
            if (exists)
            {
                return await GenerateRefreshTokenAsync();
            }
            return randomToken;
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshTokenId)
        {
            var storedToken = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.RefreshTokenValue == refreshTokenId);
            if (storedToken == null)
            {
                return false;
            }
            storedToken.IsValid = false;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task SaveRefreshTokenAsync(string userId, string jwtTokenId, string refreshToken, DateTime expiresAt)
        {
            var refreshTokenEntity = new RefreshToken
            {
                UserId = userId,
                JwtTokenId = jwtTokenId,
                RefreshTokenValue = refreshToken,
                ExpiresAt = expiresAt,
                IsValid = true
            };
            await _db.RefreshTokens.AddAsync(refreshTokenEntity);
            await _db.SaveChangesAsync();
        }

        public async Task<(bool IsValid, string? UserId, string? tokenFamilyId, bool tokenReused)> ValidateRefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.RefreshTokenValue == refreshToken);
            if (storedToken == null)
            {
                return (false, null, null, false);
            }
            if (!storedToken.IsValid)
            {
                var tokenFamily = await _db.RefreshTokens.Where(rt => rt.JwtTokenId == storedToken.JwtTokenId && rt.UserId == storedToken.UserId).ToListAsync();
                if (tokenFamily.Count > 0)
                {
                    foreach (var token in tokenFamily)
                    {
                        token.IsValid = false;
                    }
                    await _db.SaveChangesAsync();
                }
                return (false, storedToken.UserId, storedToken.JwtTokenId, true);
            }
            if (storedToken.ExpiresAt < DateTime.UtcNow)
            {
                return (false, storedToken.UserId, storedToken.JwtTokenId, false);
            }
            return (true, storedToken.UserId, storedToken.JwtTokenId, false);
        }
    }
}
