// Middleware/ExceptionHandlingMiddleware.cs - Updated
using Backend.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            _logger.LogError(exception, "An error occurred while processing {Method} request to {Path}. Query: {Query}",
                context.Request.Method, context.Request.Path, context.Request.QueryString);

            var problemDetails = new ProblemDetails
            {
                Instance = context.Request.Path
            };

            switch (exception)
            {
                case Backend.Exceptions.ValidationException validationEx:
                    HandleValidationException(problemDetails, validationEx);
                    break;
                case AuthenticationException authEx:
                    HandleAuthenticationException(problemDetails, authEx);
                    break;
                case NotFoundException notFoundEx:
                    HandleNotFoundException(problemDetails, notFoundEx);
                    break;
                case BusinessRuleException businessEx:
                    HandleBusinessRuleException(problemDetails, businessEx);
                    break;
                case UnauthorizedAccessException unauthorizedEx:
                    HandleUnauthorizedAccessException(problemDetails, unauthorizedEx);
                    break;
                case SecurityTokenExpiredException securityTokenExpiredEx:
                    HandleSecurityTokenExpiredException(problemDetails, securityTokenExpiredEx);
                    break;
                case SecurityTokenException securityTokenEx:
                    HandleSecurityTokenException(problemDetails, securityTokenEx);
                    break;
                case ArgumentNullException argumentNullEx:
                    HandleArgumentNullException(problemDetails, argumentNullEx);
                    break;
                case ArgumentException argumentEx:
                    HandleArgumentException(problemDetails, argumentEx);
                    break;
                case InvalidOperationException invalidOpEx:
                    HandleInvalidOperationException(problemDetails, invalidOpEx);
                    break;
                case DbUpdateException dbUpdateEx:
                    HandleDbUpdateException(problemDetails, dbUpdateEx);
                    break;
                case TimeoutException timeoutEx:
                    HandleTimeoutException(problemDetails, timeoutEx);
                    break;
                case HttpRequestException httpEx:
                    HandleHttpRequestException(problemDetails, httpEx);
                    break;
                default:
                    HandleDefaultException(problemDetails, exception);
                    break;
            }
            
            if (_env.IsDevelopment())
            {
                if (exception.InnerException != null)
                {
                    problemDetails.Detail += $" InnerException: {exception.InnerException.Message}";
                }
                problemDetails.Extensions["stackTrace"] = exception.StackTrace?.ToString();
            }
            else if (problemDetails.Status == StatusCodes.Status500InternalServerError)
            {
                problemDetails.Detail = "An unexpected error occurred.";
            }

            context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _env.IsDevelopment()
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, jsonOptions));
        }

        private void HandleValidationException(ProblemDetails problemDetails, Backend.Exceptions.ValidationException ex)
        {
            problemDetails.Status = StatusCodes.Status400BadRequest;
            problemDetails.Title = "Validation Failed";
            problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
            problemDetails.Detail = ex.Message;
            problemDetails.Extensions["errors"] = ex.Errors;
        }

        private void HandleAuthenticationException(ProblemDetails problemDetails, AuthenticationException ex)
        {
            problemDetails.Status = StatusCodes.Status401Unauthorized;
            problemDetails.Title = "Authentication Failed";
            problemDetails.Type = "https://tools.ietf.org/html/rfc7235#section-3.1";
            problemDetails.Detail = ex.Message;
        }

        private void HandleNotFoundException(ProblemDetails problemDetails, NotFoundException ex)
        {
            problemDetails.Status = StatusCodes.Status404NotFound;
            problemDetails.Title = "Resource Not Found";
            problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
            problemDetails.Detail = ex.Message;
        }

        private void HandleBusinessRuleException(ProblemDetails problemDetails, BusinessRuleException ex)
        {
            problemDetails.Status = StatusCodes.Status422UnprocessableEntity;
            problemDetails.Title = "Business Rule Violation";
            problemDetails.Type = "https://tools.ietf.org/html/rfc4918#section-11.2";
            problemDetails.Detail = ex.Message;
        }

        private void HandleUnauthorizedAccessException(ProblemDetails problemDetails, UnauthorizedAccessException ex)
        {
            problemDetails.Status = StatusCodes.Status403Forbidden;
            problemDetails.Title = "Access Denied";
            problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3";
            problemDetails.Detail = ex.Message;
        }

        private void HandleSecurityTokenExpiredException(ProblemDetails problemDetails, SecurityTokenExpiredException ex)
        {
            problemDetails.Status = StatusCodes.Status401Unauthorized;
            problemDetails.Title = "Token Expired";
            problemDetails.Type = "https://tools.ietf.org/html/rfc6750#section-3.1";
            problemDetails.Detail = "The token has expired. Please login again.";
        }

        private void HandleSecurityTokenException(ProblemDetails problemDetails, SecurityTokenException ex)
        {
            problemDetails.Status = StatusCodes.Status401Unauthorized;
            problemDetails.Title = "Invalid Token";
            problemDetails.Type = "https://tools.ietf.org/html/rfc6750#section-3.1";
            problemDetails.Detail = ex.Message;
        }

        private void HandleArgumentException(ProblemDetails problemDetails, ArgumentException ex)
        {
            problemDetails.Status = StatusCodes.Status400BadRequest;
            problemDetails.Title = "Invalid Argument";
            problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
            problemDetails.Detail = ex.Message;
        }

        private void HandleDefaultException(ProblemDetails problemDetails, Exception ex)
        {
            problemDetails.Status = StatusCodes.Status500InternalServerError;
            problemDetails.Title = "Internal Server Error";
            problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
            problemDetails.Detail = ex.Message;
        }

        private void HandleArgumentNullException(ProblemDetails problemDetails, ArgumentNullException ex)
        {
            problemDetails.Status = StatusCodes.Status400BadRequest;
            problemDetails.Title = "Missing Required Argument";
            problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
            problemDetails.Detail = ex.Message;
        }

        private void HandleInvalidOperationException(ProblemDetails problemDetails, InvalidOperationException ex)
        {
            problemDetails.Status = StatusCodes.Status409Conflict;
            problemDetails.Title = "Invalid Operation";
            problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8";
            problemDetails.Detail = ex.Message;
        }

        private void HandleDbUpdateException(ProblemDetails problemDetails, DbUpdateException ex)
        {
            problemDetails.Status = StatusCodes.Status500InternalServerError;
            problemDetails.Title = "Database Update Error";
            problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
            problemDetails.Detail = ex.InnerException?.Message ?? ex.Message;
        }

        private void HandleTimeoutException(ProblemDetails problemDetails, TimeoutException ex)
        {
            problemDetails.Status = StatusCodes.Status504GatewayTimeout;
            problemDetails.Title = "Operation Timed Out";
            problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.5";
            problemDetails.Detail = ex.Message;
        }

        private void HandleHttpRequestException(ProblemDetails problemDetails, HttpRequestException ex)
        {
            problemDetails.Status = StatusCodes.Status502BadGateway;
            problemDetails.Title = "HTTP Request Failed";
            problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.3";
            problemDetails.Detail = ex.Message;
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