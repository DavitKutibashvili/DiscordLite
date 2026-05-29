using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordLite_DTO
{
    public class ApiResponse<TData>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public TData? Data { get; set; }
        public object? Errors { get; set; }
        public DateTime Timestampp { get; set; } = DateTime.Now;

        public static ApiResponse<TData> Create(bool success, int statusCode, string message, TData? data = default, object? errors = null)
        {
            return new ApiResponse<TData>
            {
                Success = success,
                StatusCode = statusCode,
                Message = message,
                Data = data,
                Errors = errors
            };
        }

        public static ApiResponse<TData> Ok(TData data, string message = "Request successful")
        {
            return Create(true, 200, message, data);
        }
        public static ApiResponse<TData> CreatedAt(TData data, string message)
        {
            return Create(true, 201, message, data);
        }
        public static ApiResponse<TData> NoContent(string message = "Operation Completed Successfully")
        {
            return Create(true, 204, message);
        }
        public static ApiResponse<TData> NotFound(string message = "Resource not found")
        {
            return Create(false, 404, message);
        }
        public static ApiResponse<TData> Unauthorized(string message, object? errors = null)
        {
            return Create(false, 401, message, default, errors: errors);
        }
        public static ApiResponse<TData> BadRequest(string message, object? errors = null)
        {
            return Create(false, 400, message, default, errors: errors);
        }
        public static ApiResponse<TData> Conflict(string message, object? errors = null)
        {
            return Create(false, 409, message, default);
        }
        public static ApiResponse<TData> Error(int statuscode, string message, object? errors = null)
        {
            return Create(false, statuscode, message, default, errors);
        }
    }
}
