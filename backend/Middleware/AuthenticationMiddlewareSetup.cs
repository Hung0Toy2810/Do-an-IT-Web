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
    public class AuthenticationException : Exception
    {
        public AuthenticationException(string message) : base(message) { }
    }

    public static class AuthenticationSetup
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
        {
            var jwtKey = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key cannot be empty.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = config["Jwt:Issuer"],
                        ValidAudience = config["Jwt:Audience"],
                        IssuerSigningKey = key,
                        ClockSkew = TimeSpan.FromMinutes(5)
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            var token = context.SecurityToken as JwtSecurityToken;
                            var jti = token?.Claims?.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
                            if (string.IsNullOrEmpty(jti))
                            {
                                throw new AuthenticationException("Token is missing JTI.");
                            }

                            var redis = context.HttpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();
                            var db = redis.GetDatabase();
                            var isRevoked = await db.StringGetAsync($"revoked:{jti}");
                            if (!string.IsNullOrEmpty(isRevoked) && isRevoked == "true")
                            {
                                throw new AuthenticationException("Token has been revoked.");
                            }
                        }
                    };
                });

            return services;
        }
    }
}