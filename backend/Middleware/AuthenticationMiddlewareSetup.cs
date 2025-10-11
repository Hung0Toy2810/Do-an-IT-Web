using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Backend.Middleware
{
    public class AuthenticationException : Exception
    {
        public AuthenticationException(string message) : base(message) { }
    }

    public static class AuthenticationSetup
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
        {
            var jwtKey = config["Jwt:SecretKey"] ?? config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:SecretKey or Jwt:Key must be configured.");
            if (jwtKey.Length < 32) throw new InvalidOperationException("Jwt key must be at least 32 characters.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = config["Jwt:Issuer"] ?? "PhoneCare",
                        ValidAudience = config["Jwt:Audience"] ?? "PhoneCare",
                        IssuerSigningKey = key,
                        ClockSkew = TimeSpan.Zero,
                        NameClaimType = ClaimTypes.Name,
                        RoleClaimType = ClaimTypes.Role
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                            logger.LogDebug("OnTokenValidated triggered for request {Path}", context.Request.Path);

                            // Lấy JTI từ Principal (đã được validate và chuẩn hóa)
                            var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                            if (string.IsNullOrEmpty(jti))
                            {
                                logger.LogWarning("Token is missing JTI claim");
                                
                                // Log tất cả claims để debug
                                var allClaims = context.Principal?.Claims?.Select(c => $"{c.Type}={c.Value}") ?? Enumerable.Empty<string>();
                                logger.LogWarning("Available claims: {Claims}", string.Join(", ", allClaims));
                                
                                throw new AuthenticationException("Token is missing JTI claim.");
                            }

                            logger.LogDebug("Found JTI: {Jti}", jti);

                            try
                            {
                                var redis = context.HttpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();
                                var db = redis.GetDatabase();
                                
                                // Kiểm tra xem token có bị revoke không
                                var isRevoked = await db.StringGetAsync($"revoked:{jti}");
                                logger.LogDebug("Checked revoked:{Jti}, result: {Result}", jti, isRevoked.HasValue ? isRevoked.ToString() : "null");

                                if (isRevoked.HasValue && isRevoked == "true")
                                {
                                    logger.LogWarning("Token with JTI {Jti} was rejected because it was revoked.", jti);
                                    throw new AuthenticationException("Token has been revoked. Please login again.");
                                }

                                // Lấy userId từ claims
                                var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                                if (!string.IsNullOrEmpty(userId))
                                {
                                    // Kiểm tra session có tồn tại không
                                    var sessionKey = $"session:{userId}:{jti}";
                                    var sessionExists = await db.KeyExistsAsync(sessionKey);
                                    
                                    if (!sessionExists)
                                    {
                                        logger.LogWarning("Session not found for JTI {Jti} and UserId {UserId}", jti, userId);
                                        throw new AuthenticationException("Invalid or expired session. Please login again.");
                                    }

                                    logger.LogDebug("Valid session found for JTI {Jti} and UserId {UserId}", jti, userId);
                                }

                                // Kiểm tra role claims có hợp lệ không
                                var roleClaims = context.Principal?.FindAll(ClaimTypes.Role);
                                if (roleClaims == null || !roleClaims.Any())
                                {
                                    logger.LogWarning("Token for user {UserId} has no role claims", userId);
                                }
                            }
                            catch (AuthenticationException)
                            {
                                // Re-throw AuthenticationException để middleware tập trung xử lý
                                throw;
                            }
                            catch (RedisConnectionException ex)
                            {
                                logger.LogError(ex, "Failed to connect to Redis while checking token revocation.");
                                throw new AuthenticationException("Authentication service is temporarily unavailable. Please try again later.");
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Unexpected error during token validation.");
                                throw new AuthenticationException("Authentication failed due to an unexpected error.");
                            }
                        },
                        OnAuthenticationFailed = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                            logger.LogError(context.Exception, "Authentication failed: {Message}", context.Exception.Message);
                            
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                            
                            logger.LogWarning("Authorization challenge at {Path}: {Error}", 
                                context.Request.Path, 
                                context.ErrorDescription ?? "No token provided");
                            
                            // Chặn response mặc định và throw exception để middleware xử lý
                            context.HandleResponse();
                            throw new AuthenticationException("Unauthorized. Please provide a valid authentication token.");
                        },
                        OnForbidden = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                            
                            // Lấy thông tin user và role
                            var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                            var userRoles = context.Principal?.FindAll(ClaimTypes.Role)
                                .Select(c => c.Value)
                                .ToList() ?? new List<string>();
                            
                            var endpoint = context.HttpContext.GetEndpoint();
                            var authorizeData = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.IAuthorizeData>();
                            var requiredRoles = authorizeData?.Roles;
                            
                            logger.LogWarning(
                                "Authorization forbidden for user {UserId} with roles [{Roles}] accessing {Path}. Required roles: {RequiredRoles}",
                                userId ?? "Unknown",
                                string.Join(", ", userRoles),
                                context.Request.Path,
                                requiredRoles ?? "Not specified"
                            );
                            
                            // Chặn response mặc định và throw exception để middleware xử lý
                            throw new UnauthorizedAccessException(
                                $"You don't have permission to access this resource. Required roles: {requiredRoles ?? "Not specified"}. Your roles: {string.Join(", ", userRoles)}"
                            );
                        }
                    };
                });

            return services;
        }
    }
}