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
            var jwtKey = config["Jwt:SecretKey"] ?? config["Jwt:Key"] 
                         ?? throw new InvalidOperationException("Cấu hình Jwt:SecretKey hoặc Jwt:Key bị thiếu.");

            if (jwtKey.Length < 32) 
                throw new InvalidOperationException("Khóa JWT phải có ít nhất 32 ký tự.");

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

                            var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                            if (string.IsNullOrEmpty(jti))
                            {
                                logger.LogWarning("Token thiếu claim JTI");

                                var allClaims = context.Principal?.Claims?.Select(c => $"{c.Type}={c.Value}") ?? Enumerable.Empty<string>();
                                logger.LogWarning("Các claim hiện có: {Claims}", string.Join(", ", allClaims));

                                throw new AuthenticationException("Token không chứa mã JTI.");
                            }

                            logger.LogDebug("Found JTI: {Jti}", jti);

                            try
                            {
                                var redis = context.HttpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();
                                var db = redis.GetDatabase();

                                var isRevoked = await db.StringGetAsync($"revoked:{jti}");
                                logger.LogDebug("Checked revoked:{Jti}, result: {Result}", jti, isRevoked.HasValue ? isRevoked.ToString() : "null");

                                if (isRevoked.HasValue && isRevoked == "true")
                                {
                                    logger.LogWarning("Token với JTI {Jti} đã bị thu hồi.", jti);
                                    throw new AuthenticationException("Token đã bị thu hồi. Vui lòng đăng nhập lại.");
                                }

                                var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                                if (!string.IsNullOrEmpty(userId))
                                {
                                    var sessionKey = $"session:{userId}:{jti}";
                                    var sessionExists = await db.KeyExistsAsync(sessionKey);

                                    if (!sessionExists)
                                    {
                                        logger.LogWarning("Không tìm thấy session cho JTI {Jti}, UserId {UserId}", jti, userId);
                                        throw new AuthenticationException("Phiên đăng nhập không hợp lệ hoặc đã hết hạn. Vui lòng đăng nhập lại.");
                                    }

                                    logger.LogDebug("Session hợp lệ cho JTI {Jti}, UserId {UserId}", jti, userId);
                                }

                                var roleClaims = context.Principal?.FindAll(ClaimTypes.Role);
                                if (roleClaims == null || !roleClaims.Any())
                                {
                                    logger.LogWarning("Token của user {UserId} không có role claims", userId);
                                }
                            }
                            catch (AuthenticationException)
                            {
                                throw;
                            }
                            catch (RedisConnectionException ex)
                            {
                                logger.LogError(ex, "Lỗi kết nối Redis khi kiểm tra token.");
                                throw new AuthenticationException("Hệ thống xác thực tạm thời không khả dụng. Vui lòng thử lại sau.");
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Lỗi không mong muốn khi xác thực token.");
                                throw new AuthenticationException("Xác thực thất bại do lỗi không mong muốn.");
                            }
                        },

                        OnAuthenticationFailed = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                            logger.LogError(context.Exception, "Xác thực thất bại: {Message}", context.Exception.Message);
                            return Task.CompletedTask;
                        },

                        OnChallenge = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();

                            logger.LogWarning("Challenge tại {Path}: {Error}",
                                context.Request.Path,
                                context.ErrorDescription ?? "Không có token");

                            context.HandleResponse();
                            throw new AuthenticationException("Không có quyền truy cập. Vui lòng cung cấp token hợp lệ.");
                        },

                        OnForbidden = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();

                            var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                            var userRoles = context.Principal?.FindAll(ClaimTypes.Role)
                                .Select(c => c.Value)
                                .ToList() ?? new List<string>();

                            var endpoint = context.HttpContext.GetEndpoint();
                            var authorizeData = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.IAuthorizeData>();
                            var requiredRoles = authorizeData?.Roles;

                            logger.LogWarning(
                                "Cấm truy cập: user {UserId} roles [{Roles}] -> {Path}. Yêu cầu roles: {RequiredRoles}",
                                userId ?? "Không xác định",
                                string.Join(", ", userRoles),
                                context.Request.Path,
                                requiredRoles ?? "Không yêu cầu"
                            );

                            throw new UnauthorizedAccessException(
                                $"Bạn không có quyền truy cập tài nguyên này. Quyền yêu cầu: {requiredRoles ?? "Không yêu cầu"}. Quyền của bạn: {string.Join(", ", userRoles)}"
                            );
                        }
                    };
                });

            return services;
        }
    }
}
