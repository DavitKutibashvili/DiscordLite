using AutoMapper;
using DiscordLite_API.Data;
using DiscordLite_API.Model;
using DiscordLite_API.Services.IServices;
using DiscordLite_DTO;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;

namespace DiscordLite_API.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public AuthService(ApplicationDbContext db, IConfiguration configuration, IMapper mapper, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ITokenService tokenService)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _roleManager = roleManager;
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<TokenDTO?> LoginAsync(LoginRequestDTO loginRequestDTO)
        {
            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == loginRequestDTO.UsernameOrEmail.ToUpper() || u.NormalizedUserName == loginRequestDTO.UsernameOrEmail.ToUpper());
                if (user == null)
                {
                    return null;
                }
                bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);
                if (!isValid)
                {
                    return null;
                }
                var token = await _tokenService.GenerateJwtTokenAsync(user);

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var jwtTokenId = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;


                var refreshToken = await _tokenService.GenerateRefreshTokenAsync();
                var refreshTokenExpiry = DateTime.UtcNow.AddMinutes(60);
                await _tokenService.SaveRefreshTokenAsync(user.Id, jwtTokenId, refreshToken, refreshTokenExpiry);

                TokenDTO tokenDTO = new()
                {
                    AccessToken = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = jwtToken.ValidTo
                };
                return tokenDTO;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while logging in: {ex.Message}");
            }
        }

        public async Task<(UserDTO?, string error)> RegisterAsync(RegistrationRequestDTO registrationRequestDTO)
        {
            try
            {
                // Check email
                var emailExists = await _userManager.FindByEmailAsync(registrationRequestDTO.Email) != null;
                if (emailExists)
                    return (null, $"Account with email {registrationRequestDTO.Email} already exists");

                // Check username
                var usernameExists = await _userManager.FindByNameAsync(registrationRequestDTO.UserName) != null;
                if (usernameExists)
                    return (null, $"Account with username {registrationRequestDTO.UserName} already exists");

                User user = new()
                {
                    Email = registrationRequestDTO.Email,
                    DisplayName = registrationRequestDTO.DisplayName,
                    UserName = registrationRequestDTO.UserName,
                    NormalizedEmail = registrationRequestDTO.Email.ToUpper(),
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, registrationRequestDTO.Password);
                if (!result.Succeeded)
                    throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

                await _userManager.AddToRoleAsync(user, "User");

                return (_mapper.Map<UserDTO>(user), null);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error during registration: {ex.Message}");
            }
        }
        public async Task<TokenDTO?> RefreshAccessTokenAsync(RefreshTokenRequestDTO refreshTokenRequestDTO)
        {
            try
            {
                if (string.IsNullOrEmpty(refreshTokenRequestDTO.RefreshToken))
                {
                    return null;
                }
                var (isValid, userId, TokenFamilyId, tokenReused) = await _tokenService.ValidateRefreshTokenAsync(refreshTokenRequestDTO.RefreshToken);
                if (tokenReused)
                {
                    return null;
                }
                if (!isValid || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(TokenFamilyId))
                {
                    return null;
                }
                var user = await _db.Users.FindAsync(userId);
                if (user == null)
                {
                    return null;
                }
                await _tokenService.RevokeRefreshTokenAsync(refreshTokenRequestDTO.RefreshToken);
                var token = await _tokenService.GenerateJwtTokenAsync(user);

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);


                var refreshToken = await _tokenService.GenerateRefreshTokenAsync();
                var refreshTokenExpiry = DateTime.UtcNow.AddMinutes(60);
                await _tokenService.SaveRefreshTokenAsync(user.Id, TokenFamilyId, refreshToken, refreshTokenExpiry);

                TokenDTO tokenDTO = new()
                {
                    AccessToken = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = jwtToken.ValidTo
                };
                return tokenDTO;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while refreshing access token: {ex.Message}");
            }
        }
    }
}
