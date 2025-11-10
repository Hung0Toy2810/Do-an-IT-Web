using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;

namespace Backend.Service.ViettelPost
{
    public class ViettelPostTokenService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<ViettelPostTokenService> _logger;
        private readonly IDatabase _redis;
        private readonly HttpClient _httpClient;

        private const string TOKEN_KEY = "viettelpost:token";
        private const string EXPIRE_KEY = "viettelpost:expire";

        private readonly string _baseUrl;
        private readonly string _username;
        private readonly string _password;
        private readonly int _bufferMinutes;

        public ViettelPostTokenService(
            IConfiguration config,
            ILogger<ViettelPostTokenService> logger,
            IConnectionMultiplexer redis)
        {
            _config = config;
            _logger = logger;
            _redis = redis.GetDatabase();
            _httpClient = new HttpClient();

            _baseUrl = _config["ViettelPost:BaseUrl"]!;
            _username = _config["ViettelPost:Username"]!;
            _password = _config["ViettelPost:Password"]!;
            _bufferMinutes = _config.GetValue<int>("ViettelPost:RefreshBufferMinutes", 5);
        }

        public async Task<string> GetValidTokenAsync()
        {
            var tokenValue = await _redis.StringGetAsync(TOKEN_KEY);
            var expireStr = await _redis.StringGetAsync(EXPIRE_KEY);

            if (tokenValue.IsNullOrEmpty || string.IsNullOrEmpty(expireStr) || !long.TryParse(expireStr, out long expireMs))
            {
                _logger.LogWarning("Token không tồn tại hoặc expire không hợp lệ trong Redis. Đăng nhập mới.");
                return await LoginAndSaveAsync();
            }

            var expireTime = DateTimeOffset.FromUnixTimeMilliseconds(expireMs);
            var refreshTime = expireTime.AddMinutes(-_bufferMinutes);

            if (DateTimeOffset.UtcNow >= refreshTime)
            {
                _logger.LogInformation("Token sắp hết hạn (còn < {Buffer} phút). Tự động refresh.", _bufferMinutes);
                return await LoginAndSaveAsync();
            }

            return tokenValue.ToString();
        }

        private async Task<string> LoginAndSaveAsync()
        {
            var request = new { USERNAME = _username, PASSWORD = _password };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                Console.WriteLine($"[ViettelPost] Đang đăng nhập với USERNAME: {_username}...");
                _logger.LogInformation("Đang gọi API đăng nhập Viettel Post...");

                var response = await _httpClient.PostAsync($"{_baseUrl}/v2/user/Login", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[ViettelPost] Đăng nhập thất bại! Status: {response.StatusCode}");
                    _logger.LogError("Login Viettel Post thất bại: {Status} - {Body}", response.StatusCode, responseBody);
                    throw new InvalidOperationException("Không thể đăng nhập Viettel Post");
                }

                var result = JsonSerializer.Deserialize<LoginResponse>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result?.Data?.Token == null)
                {
                    Console.WriteLine($"[ViettelPost] Token không có trong response!");
                    _logger.LogError("Token không có trong response: {Body}", responseBody);
                    throw new InvalidOperationException("Token Viettel Post không hợp lệ");
                }

                var token = result.Data.Token;
                var expireMs = result.Data.Expired;
                var expireTime = DateTimeOffset.FromUnixTimeMilliseconds(expireMs).ToLocalTime();
                var remainingMinutes = (expireMs - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) / 60000;

                // IN RA CONSOLE ĐẸP - TOÀN BỘ TOKEN
                Console.WriteLine("─────────────────────────────────────────────");
                Console.WriteLine("[ViettelPost] ĐĂNG NHẬP THÀNH CÔNG!");
                Console.WriteLine($"   USERNAME : {_username}");
                Console.WriteLine($"   TOKEN    : {token}");
                Console.WriteLine($"   HẾT HẠN  : {expireTime:yyyy-MM-dd HH:mm:ss} (còn {remainingMinutes} phút)");
                Console.WriteLine("─────────────────────────────────────────────");

                var ttl = TimeSpan.FromMilliseconds(Math.Max(1, expireMs - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
                await _redis.StringSetAsync(TOKEN_KEY, token, ttl);
                await _redis.StringSetAsync(EXPIRE_KEY, expireMs.ToString());

                _logger.LogInformation("Token Viettel Post đã được cập nhật. Hết hạn: {Expire}", expireTime);
                return token;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ViettelPost] LỖI ĐĂNG NHẬP: {ex.Message}");
                _logger.LogError(ex, "Lỗi khi login Viettel Post");
                throw;
            }
        }
    }

    public class LoginResponse
    {
        public int Status { get; set; }
        public bool Error { get; set; }
        public string Message { get; set; } = string.Empty;
        public LoginData? Data { get; set; }
    }

    public class LoginData
    {
        public string Token { get; set; } = string.Empty;
        public long Expired { get; set; } // Unix ms
    }
}