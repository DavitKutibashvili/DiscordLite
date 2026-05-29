using DiscordLite_API.Services.IServices;
using DiscordLite_DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DiscordLite_API.Controllers
{
    [Route("api/auth")]
    [ApiVersionNeutral]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<UserDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<UserDTO>>> Register([FromBody] RegistrationRequestDTO registrationRequestDTO)
        {
            try
            {
                if (registrationRequestDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Registration data is required"));
                }
                var (user, error) = await _authService.RegisterAsync(registrationRequestDTO);
                if(error != null)
                {
                    return Conflict(ApiResponse<object>.Conflict(error));
                }
                if (user == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Failed to create user account"));
                }

                var response = ApiResponse<UserDTO>.CreatedAt(user, "User registered successfully");
                return CreatedAtAction(nameof(Register), response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occurred while processing the registration", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<TokenDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<TokenDTO>>> Login([FromBody] LoginRequestDTO loginRequestDTO)
        {
            try
            {
                if (loginRequestDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Login data is required"));
                }
                var loginResponse = await _authService.LoginAsync(loginRequestDTO);
                if (loginResponse == null)
                {
                    return Unauthorized(ApiResponse<object>.Unauthorized("Invalid username/email or password"));
                }

                var response = ApiResponse<TokenDTO>.Ok(loginResponse, "User logged in successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occurred while logging in", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<TokenDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<TokenDTO>>> RefreshAccessToken([FromBody] RefreshTokenRequestDTO refreshTokenRequestDTO)
        {
            try
            {
                if (refreshTokenRequestDTO == null || string.IsNullOrEmpty(refreshTokenRequestDTO.RefreshToken))
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Refresh token is required"));
                }
                var tokenResponse = await _authService.RefreshAccessTokenAsync(refreshTokenRequestDTO);

                if (tokenResponse == null)
                {
                    return Unauthorized(ApiResponse<object>.Unauthorized("Invalid or expired refresh token"));
                }

                var response = ApiResponse<TokenDTO>.Ok(tokenResponse, "Token refreshed successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occurred while refreshing token", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }
    }
}
