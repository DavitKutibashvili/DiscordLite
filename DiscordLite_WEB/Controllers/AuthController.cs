using AutoMapper;
using DiscordLite_DTO;
using DiscordLite_WEB.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace DiscordLite_WEB.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;
        private readonly ITokenProvider _tokenProvider;
        public AuthController(IAuthService authService, IMapper mapper, ITokenProvider tokenProvider)
        {
            _authService = authService;
            _mapper = mapper;
            _tokenProvider = tokenProvider;
        }
        [HttpGet]
        public async Task<IActionResult> Login()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _tokenProvider.ClearToken();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestDTO loginRequestDTO)
        {
            try
            {
                var response = await _authService.LoginAsync<ApiResponse<TokenDTO>>(loginRequestDTO);
                if (response != null && response.Success && response.Data != null)
                {
                    var principal = _tokenProvider.CreatePrincipalFromJWTToken(response.Data.AccessToken);
                    if (principal != null)
                    {
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                        _tokenProvider.SetToken(response.Data.AccessToken, response.Data.RefreshToken);
                        return RedirectToAction("Index", "Chat");
                    }
                    else
                    {
                        TempData["error"] = "Invalid token received.";
                    }
                }
                else
                {
                    TempData["error"] = response?.Message ?? "Login failed. Please try again.";
                    return View(loginRequestDTO);
                }
                return View(loginRequestDTO);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"an error occured: {ex.Message}";
            }
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _tokenProvider.ClearToken();
            return View(new RegistrationRequestDTO
            {
                Email = string.Empty,
                UserName = string.Empty,
                DisplayName = string.Empty,
                Password = string.Empty
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistrationRequestDTO registrationRequestDTO)
        {
            try
            {
                ApiResponse<UserDTO> response = await _authService.RegisterAsync<ApiResponse<UserDTO>>(registrationRequestDTO);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Registration Succesful";
                    return RedirectToAction(nameof(Login));
                }
                else
                {
                    TempData["error"] = response?.Message ?? "Registration failed";
                    return View(registrationRequestDTO);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"an error occured: {ex.Message}";
                return View(registrationRequestDTO);
            }
        }
        public async Task<IActionResult> AccessDenied()
        {
            return View();
        }
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            _tokenProvider.ClearToken();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult GetCurrentToken()
        {
            return Ok(_tokenProvider.GetAccessToken());
        }
    }
}
