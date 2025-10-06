using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using StackExchange.Redis;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace Backend.Service.Token
{
    public interface IJwtTokenService
    {
        Task<string> GenerateTokenAsync(string id, string username, string role, string clientIp);
        Task RevokeTokenAsync(string token);
        Task RevokeAllTokensExceptCurrentAsync(string userId, string currentTokenJti);
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _config;
        private readonly IConnectionMultiplexer _redis;
        private readonly SymmetricSecurityKey _key;
        private readonly ILogger<JwtTokenService> _logger;

        public JwtTokenService(
            IConfiguration config,
            IConnectionMultiplexer redis,
            ILogger<JwtTokenService> logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
            if (jwtKey.Length < 32) throw new InvalidOperationException("Jwt:Key must be at least 32 characters.");
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        }

        public async Task<string> GenerateTokenAsync(string id, string username, string role, string clientIp)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(username))
                throw new ArgumentException("User ID and username cannot be empty.");

            string GenerateSecureRandomString(int byteLength)
            {
                byte[] randomBytes = new byte[byteLength];
                RandomNumberGenerator.Fill(randomBytes);
                return Convert.ToBase64String(randomBytes)
                    .Replace("+", "-")
                    .Replace("/", "_")
                    .TrimEnd('=');
            }

            var jti = GenerateSecureRandomString(32);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, id),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, jti),
                new Claim("client_ip", clientIp ?? "unknown")
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: new SigningCredentials(_key, SecurityAlgorithms.HmacSha256)
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            var db = _redis.GetDatabase();
            var sessionKey = $"session:{id}:{jti}";
            var sessionData = $"{jti}|{clientIp}|{username}|{role}|{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            await db.StringSetAsync(sessionKey, sessionData, TimeSpan.FromDays(7));
            await db.ListRightPushAsync($"tokens:{id}", jti);

            _logger.LogInformation("Generated token for user {Username} with JTI {Jti}", username, jti);
            return accessToken;
        }

        public async Task RevokeTokenAsync(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            if (!tokenHandler.CanReadToken(token))
            {
                _logger.LogWarning("Invalid token format.");
                throw new SecurityTokenException("Invalid token format.");
            }

            var jwtToken = tokenHandler.ReadJwtToken(token);
            var jtiClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (jtiClaim == null || userIdClaim == null)
            {
                _logger.LogWarning("Token does not contain JTI or UserId.");
                throw new SecurityTokenException("Invalid token: JTI or UserId missing.");
            }

            var jti = jtiClaim.Value;
            var userId = userIdClaim.Value;
            var ttl = jwtToken.ValidTo - DateTime.UtcNow;
            if (ttl <= TimeSpan.Zero)
            {
                _logger.LogWarning("Token with JTI {Jti} is already expired.", jti);
                return;
            }

            var db = _redis.GetDatabase();
            await db.StringSetAsync($"revoked:{jti}", "true", ttl);
            await db.KeyDeleteAsync($"session:{userId}:{jti}");
            await db.ListRemoveAsync($"tokens:{userId}", jti);

            _logger.LogInformation("Revoked token with JTI {Jti} for user {UserId}", jti, userId);
        }

        public async Task RevokeAllTokensExceptCurrentAsync(string userId, string currentTokenJti)
        {
            var db = _redis.GetDatabase();
            var tokenListKey = $"tokens:{userId}";
            var jtids = await db.ListRangeAsync(tokenListKey);
            if (jtids.Length == 0)
            {
                _logger.LogInformation("No tokens found for user {UserId}", userId);
                return;
            }

            foreach (var jti in jtids)
            {
                if (jti != currentTokenJti)
                {
                    await db.StringSetAsync($"revoked:{jti}", "true", TimeSpan.FromDays(7));
                    await db.KeyDeleteAsync($"session:{userId}:{jti}");
                }
            }

            await db.KeyDeleteAsync(tokenListKey);
            if (!string.IsNullOrEmpty(currentTokenJti))
            {
                await db.ListRightPushAsync(tokenListKey, currentTokenJti);
            }

            _logger.LogInformation("Revoked all tokens except JTI {CurrentJti} for user {UserId}", currentTokenJti, userId);
        }
    }
}