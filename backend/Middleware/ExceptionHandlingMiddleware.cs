using Backend.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An error occurred while processing {Method} request to {Path}",
                context.Request.Method, context.Request.Path);

            var (statusCode, message) = GetStatusCodeAndMessage(exception);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var response = new
            {
                Message = message
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }

        private (int statusCode, string message) GetStatusCodeAndMessage(Exception exception)
        {
            return exception switch
            {
                Backend.Exceptions.ValidationException ex => 
                    (StatusCodes.Status400BadRequest, ex.Message),
                
                AuthenticationException ex => 
                    (StatusCodes.Status401Unauthorized, ex.Message),
                
                SecurityTokenExpiredException ex => 
                    (StatusCodes.Status401Unauthorized, ex.Message),
                
                SecurityTokenException ex => 
                    (StatusCodes.Status401Unauthorized, ex.Message),
                
                UnauthorizedAccessException ex => 
                    (StatusCodes.Status403Forbidden, ex.Message),
                
                NotFoundException ex => 
                    (StatusCodes.Status404NotFound, ex.Message),
                
                InvalidOperationException ex => 
                    (StatusCodes.Status409Conflict, ex.Message),
                
                BusinessRuleException ex => 
                    (StatusCodes.Status422UnprocessableEntity, ex.Message),
                
                ArgumentNullException ex => 
                    (StatusCodes.Status400BadRequest, ex.Message),
                
                ArgumentException ex => 
                    (StatusCodes.Status400BadRequest, ex.Message),
                
                DbUpdateException ex => 
                    (StatusCodes.Status500InternalServerError, ex.InnerException?.Message ?? ex.Message),
                
                TimeoutException ex => 
                    (StatusCodes.Status504GatewayTimeout, ex.Message),
                
                HttpRequestException ex => 
                    (StatusCodes.Status502BadGateway, ex.Message),
                
                _ => (StatusCodes.Status500InternalServerError, exception.Message)
            };
        }
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}