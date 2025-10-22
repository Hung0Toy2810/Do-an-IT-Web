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

        public JwtTokenService(IConfiguration config, IConnectionMultiplexer redis, ILogger<JwtTokenService> logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config), "Cấu hình không được null");
            _redis = redis ?? throw new ArgumentNullException(nameof(redis), "Redis không được null");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger không được null");
            var jwtKey = _config["Jwt:SecretKey"] ?? _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:SecretKey hoặc Jwt:Key phải được cấu hình.");
            if (jwtKey.Length < 32) throw new InvalidOperationException("Khóa JWT phải dài ít nhất 32 ký tự.");
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        }

        public async Task<string> GenerateTokenAsync(string id, string username, string role, string clientIp)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(username))
                throw new ArgumentException("ID người dùng và tên người dùng không được rỗng.");

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
                new Claim("jti", jti),
                new Claim("client_ip", clientIp ?? "unknown")
            };

            var expirationMinutes = _config.GetValue<int>("Jwt:ExpirationMinutes", 1440);
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: new SigningCredentials(_key, SecurityAlgorithms.HmacSha256)
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            var db = _redis.GetDatabase();
            var sessionKey = $"session:{id}:{jti}";
            var sessionData = $"{jti}|{clientIp}|{username}|{role}|{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            await db.StringSetAsync(sessionKey, sessionData, TimeSpan.FromMinutes(expirationMinutes));
            await db.ListRightPushAsync($"tokens:{id}", jti);

            _logger.LogInformation("Đã tạo token cho người dùng {Username} với JTI {Jti}", username, jti);
            return accessToken;
        }

        public async Task RevokeTokenAsync(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            if (!tokenHandler.CanReadToken(token))
            {
                _logger.LogWarning("Định dạng token không hợp lệ.");
                throw new SecurityTokenException("Định dạng token không hợp lệ.");
            }

            var jwtToken = tokenHandler.ReadJwtToken(token);
            var jtiClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "jti");
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (jtiClaim == null || userIdClaim == null)
            {
                _logger.LogWarning("Token không chứa JTI hoặc UserId.");
                throw new SecurityTokenException("Token không hợp lệ: Thiếu JTI hoặc UserId.");
            }

            var jti = jtiClaim.Value;
            var userId = userIdClaim.Value;
            var ttl = jwtToken.ValidTo - DateTime.UtcNow;
            if (ttl <= TimeSpan.Zero)
            {
                _logger.LogWarning("Token với JTI {Jti} đã hết hạn.", jti);
                return;
            }

            var db = _redis.GetDatabase();
            await db.StringSetAsync($"revoked:{jti}", "true", ttl);
            await db.KeyDeleteAsync($"session:{userId}:{jti}");
            await db.ListRemoveAsync($"tokens:{userId}", jti);

            _logger.LogInformation("Đã thu hồi token với JTI {Jti} cho người dùng {UserId}, TTL: {TTL}", jti, userId, ttl);
        }

        public async Task RevokeAllTokensExceptCurrentAsync(string userId, string currentTokenJti)
        {
            var db = _redis.GetDatabase();
            var tokens = await db.ListRangeAsync($"tokens:{userId}");
            foreach (var tokenJti in tokens)
            {
                if (tokenJti != currentTokenJti)
                {
                    await db.StringSetAsync($"revoked:{tokenJti}", "true", TimeSpan.FromMinutes(_config.GetValue<int>("Jwt:ExpirationMinutes", 1440)));
                    await db.KeyDeleteAsync($"session:{userId}:{tokenJti}");
                    await db.ListRemoveAsync($"tokens:{userId}", tokenJti);
                    _logger.LogInformation("Đã thu hồi token với JTI {Jti} cho người dùng {UserId}", tokenJti, userId);
                }
            }
        }
    }
}