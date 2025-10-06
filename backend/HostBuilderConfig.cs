using Microsoft.EntityFrameworkCore;
using Minio;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;
using Backend.SQLDbContext; // 👈 thêm using này để nhận SQLServerDbContext

namespace Backend 
{
    public static class HostBuilderConfig
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;

                    // ===== SQL Server: DbContext =====
                    var connectionString = configuration.GetConnectionString("DefaultConnection");
                    if (string.IsNullOrEmpty(connectionString))
                        throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

                    services.AddDbContext<SQLServerDbContext>(options =>
                        options.UseSqlServer(connectionString)
                               .UseLazyLoadingProxies());

                    // ===== MongoDb Factory =====
                    services.AddSingleton<IMongoDbContextFactory, MongoDbContextFactory>();

                    // ===== Redis =====
                    try
                    {
                        var redisConfig = ConfigurationOptions.Parse($"{configuration["Redis:Host"]}:{configuration["Redis:Port"]}");
                        redisConfig.Password = configuration["Redis:Password"];
                        redisConfig.AbortOnConnectFail = false;

                        var redis = ConnectionMultiplexer.Connect(redisConfig);
                        services.AddSingleton<IConnectionMultiplexer>(redis);
                    }
                    catch (RedisConnectionException ex)
                    {
                        var logger = services.BuildServiceProvider().GetService<ILoggerFactory>()?.CreateLogger("Redis");
                        logger?.LogError(ex, "Failed to connect to Redis at {Host}:{Port}", configuration["Redis:Host"], configuration["Redis:Port"]);
                        throw;
                    }

                    // ===== Minio =====
                    services.AddSingleton<IMinioClient>(sp =>
                    {
                        var minioConfig = configuration.GetSection("Minio");
                        return new MinioClient()
                            .WithEndpoint(minioConfig["Endpoint"])
                            .WithCredentials(minioConfig["AccessKey"], minioConfig["SecretKey"])
                            .WithSSL(minioConfig.GetValue<bool>("Secure"))
                            .Build();
                    });

                    // ===== Controllers =====
                    services.AddControllers();

                    // ===== CORS =====
                    services.AddCors(options =>
                    {
                        options.AddPolicy("AllowAll", policy =>
                        {
                            policy.AllowAnyOrigin()
                                  .AllowAnyMethod()
                                  .AllowAnyHeader();
                        });
                    });

                    // ===== Logging =====
                    services.AddLogging(builder =>
                    {
                        builder.AddConsole();
                        builder.SetMinimumLevel(LogLevel.Information);
                    });

                    // ===== Đăng ký Service nội bộ =====
                    // 👉 Thêm các service cho dự án tại đây
                    // services.AddScoped<IProductService, ProductService>();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.Configure(app =>
                    {
                        app.UseRouting();

                        // Log request headers
                        app.Use(async (context, next) =>
                        {
                            Console.WriteLine("Request Headers: " + string.Join(", ",
                                context.Request.Headers.Select(h => $"{h.Key}: {h.Value}")));
                            await next();
                        });

                        app.UseCors("AllowAll");

                        app.UseAuthentication();
                        app.UseAuthorization();

                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllers();
                        });
                    });
                });
    }
}
