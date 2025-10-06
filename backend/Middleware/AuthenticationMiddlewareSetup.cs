using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Middleware
{
    // Custom exception cho lỗi xác thực
    public class AuthenticationException : Exception
    {
        public AuthenticationException(string message) : base(message) { }
    }

    public static class AuthenticationSetup
    {
        // Extension method để cấu hình JWT Authentication trong IServiceCollection
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
        {
            // Lấy khóa bí mật JWT từ appsettings.json
            var jwtKey = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key cannot be empty.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            services
                // Đăng ký dịch vụ Authentication với scheme là JWT Bearer
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    // Cấu hình các tham số để validate token
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true, // Kiểm tra Issuer có hợp lệ hay không
                        ValidateAudience = true, // Kiểm tra Audience có hợp lệ hay không
                        ValidateLifetime = true, // Kiểm tra token có hết hạn chưa
                        ValidateIssuerSigningKey = true, // Kiểm tra chữ ký token
                        ValidIssuer = config["Jwt:Issuer"], // Issuer hợp lệ
                        ValidAudience = config["Jwt:Audience"], // Audience hợp lệ
                        IssuerSigningKey = key, // Khóa bí mật để xác thực chữ ký
                        ClockSkew = TimeSpan.FromMinutes(5) // Cho phép lệch thời gian 5 phút
                    };

                    // Định nghĩa sự kiện khi JWT được validate thành công
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            // Lấy JWT từ context
                            var token = context.SecurityToken as JwtSecurityToken;

                            // Lấy claim JTI (ID duy nhất của token)
                            var jti = token?.Claims?.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
                            if (string.IsNullOrEmpty(jti))
                            {
                                // Nếu token không có JTI thì coi như không hợp lệ
                                throw new AuthenticationException("Token is missing JTI.");
                            }

                            // Kết nối đến Redis để kiểm tra token có bị revoke hay không
                            var redis = context.HttpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();
                            var db = redis.GetDatabase();

                            // Kiểm tra trong Redis xem token đã bị đánh dấu revoked chưa
                            var isRevoked = await db.StringGetAsync($"revoked:{jti}");
                            if (!string.IsNullOrEmpty(isRevoked) && isRevoked == "true")
                            {
                                // Nếu token đã bị revoke thì ném lỗi
                                throw new AuthenticationException("Token has been revoked.");
                            }
                        }
                    };
                });

            return services;
        }
    }
}
