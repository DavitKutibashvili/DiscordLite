using DiscordLite_DTO;
using DiscordLite_Utility;
using DiscordLite_WEB.Models;
using DiscordLite_WEB.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using static DiscordLite_Utility.SD;

namespace DiscordLite_WEB.Services
{
    public class BaseService: IBaseService
    {
        public IHttpClientFactory _HttpClient { get; set; }
        private readonly ITokenProvider _tokenProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string RefreshingTokenKey = "_RefreshingToken";
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
        public ApiResponse<object> ResponseModel { get; set; }
        public BaseService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider, IHttpContextAccessor httpContextAccessor)
        {
            this.ResponseModel = new();
            _tokenProvider = tokenProvider;
            _HttpClient = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }
        private bool IsRefreshingToken
        {
            get => _httpContextAccessor.HttpContext?.Session.GetString(RefreshingTokenKey) == "true";
            set
            {
                if (value)
                {
                    _httpContextAccessor.HttpContext?.Session.SetString(RefreshingTokenKey, "true");
                }
                else
                {
                    _httpContextAccessor.HttpContext?.Session.Remove(RefreshingTokenKey);
                }
            }
        }
        public async Task<T?> SendAsync<T>(ApiRequest apiRequest, bool withBearer = true)
        {
            try
            {
                var client = _HttpClient.CreateClient("DiscordLiteAPI");
                var message = CreateRequestMessage(apiRequest, withBearer);
                var apiResponse = await client.SendAsync(message);

                if (apiResponse.StatusCode == HttpStatusCode.Unauthorized && withBearer && !IsRefreshingToken)
                {
                    // Handle unauthorized access, e.g., by returning a specific error response or triggering a token refresh
                    Console.WriteLine("Unauthorized access - token may be invalid or expired.");
                    var refreshed = await RefreshAccessToken();
                    if (refreshed)
                    {
                        var retryMessage = CreateRequestMessage(apiRequest, withBearer);
                        apiResponse = await client.SendAsync(retryMessage);
                    }
                    else
                    {
                        _tokenProvider.ClearToken();
                        await _httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                        _httpContextAccessor.HttpContext?.Response.Redirect("/auth/login");
                        return default;
                    }
                }

                if (!apiResponse.IsSuccessStatusCode)
                {
                    // Attempt to deserialize an ApiResponse<T> if possible to provide structured error info
                    try
                    {
                        var errorBody = await apiResponse.Content.ReadFromJsonAsync<T>(JsonOptions);
                        return errorBody;
                    }
                    catch
                    {
                        // If deserialization fails or body is empty, return default (null) so callers must handle non-success responses
                        return default;
                    }
                }

                var content = await apiResponse.Content.ReadFromJsonAsync<T>(JsonOptions);
                return content;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return default;
            }
        }
        private async Task<bool> RefreshAccessToken()
        {
            try
            {
                if (IsRefreshingToken)
                {
                    await Task.Delay(1000);
                    var accessToken = _tokenProvider.GetAccessToken();
                    if (accessToken != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                IsRefreshingToken = true;
                var refreshToken = _tokenProvider.GetRefreshToken();
                if (refreshToken == null)
                {
                    return false;
                }
                var client = _HttpClient.CreateClient("DiscordLiteAPI");
                var refreshRequest = new RefreshTokenRequestDTO
                {
                    RefreshToken = refreshToken
                };
                var apiRequest = new ApiRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = refreshRequest,
                    Url = "/api/auth/refresh-token"
                };
                var message = CreateRequestMessage(apiRequest, withBearer: false);
                var response = await client.SendAsync(message);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<TokenDTO>>();
                    if (result?.Success == true && result.Data != null && !string.IsNullOrEmpty(result.Data.AccessToken) && !string.IsNullOrEmpty(result.Data.RefreshToken))
                    {
                        _tokenProvider.SetToken(result.Data.AccessToken, result.Data.RefreshToken);
                        return true;
                    }
                }
                _tokenProvider.ClearToken();
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token refresh error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                _tokenProvider.ClearToken();
                return false;
            }
            finally
            {
                IsRefreshingToken = false;
            }
        }
        private HttpRequestMessage CreateRequestMessage(ApiRequest apiRequest, bool withBearer)
        {
            var message = new HttpRequestMessage()
            {
                RequestUri = new Uri(apiRequest.Url, UriKind.Relative),
                Method = GetHttpMethod(apiRequest.ApiType)
            };

            var token = _tokenProvider.GetAccessToken();
            if (withBearer && !string.IsNullOrEmpty(token))
            {
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            if (apiRequest.Data != null)
            {
                if (apiRequest.Data is MultipartFormDataContent formData)
                {
                    message.Content = formData;
                }
                else
                {
                    message.Content = JsonContent.Create(apiRequest.Data, options: JsonOptions);
                }
            }
            return message;
        }
        private static HttpMethod GetHttpMethod(ApiType apiType)
        {
            return apiType switch
            {
                ApiType.POST => HttpMethod.Post,
                ApiType.PUT => HttpMethod.Put,
                ApiType.DELETE => HttpMethod.Delete,
                _ => HttpMethod.Get,
            };
        }
    }
}
