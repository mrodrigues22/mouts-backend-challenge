using Ambev.DeveloperEvaluation.WebApi.Common;
using FluentValidation;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware
{
    public class ValidationExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ValidationExceptionMiddleware> _logger;

        public ValidationExceptionMiddleware(RequestDelegate next, ILogger<ValidationExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                var detail = string.Join(" ", ex.Errors.Select(e => e.ErrorMessage));
                await WriteResponseAsync(context, StatusCodes.Status400BadRequest, "ValidationError", "Invalid input data", detail);
            }
            catch (KeyNotFoundException ex)
            {
                await WriteResponseAsync(context, StatusCodes.Status404NotFound, "ResourceNotFound", "Resource not found", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await WriteResponseAsync(context, StatusCodes.Status409Conflict, "ConflictError", "Operation conflict", ex.Message);
            }
            catch (DomainException ex)
            {
                await WriteResponseAsync(context, StatusCodes.Status400BadRequest, "DomainError", "Business rule violation", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
                await WriteResponseAsync(context, StatusCodes.Status500InternalServerError, "InternalServerError", "An unexpected error occurred", "Please try again later.");
            }
        }

        private static Task WriteResponseAsync(HttpContext context, int statusCode, string type, string error, string detail)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new ErrorResponse
            {
                Type = type,
                Error = error,
                Detail = detail
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
    }
}
