using Microsoft.EntityFrameworkCore;
using Minio;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;
using Backend.SQLDbContext;
using Backend.Repository.AdministratorRepository;
using Backend.Repository.CustomerRepository;
using Backend.Repository.MinIO;
using Backend.Service.AdministratorService;
using Backend.Service.CustomerService;
using Backend.Service.Password;
using Backend.Service.Token;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Backend.Middleware;

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

                    // ===== JWT Authentication =====
                    var jwtSecretKey = configuration["Jwt:SecretKey"];
                    if (string.IsNullOrEmpty(jwtSecretKey))
                    {
                        jwtSecretKey = "DefaultSecretKeyForDevelopmentOnlyMustBeAtLeast32Characters!";
                        Console.WriteLine("⚠️  WARNING: Using default JWT SecretKey. Please configure Jwt:SecretKey in appsettings.json");
                    }

                    services.AddAuthentication(options =>
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
                            ValidIssuer = configuration["Jwt:Issuer"] ?? "BackendAPI",
                            ValidAudience = configuration["Jwt:Audience"] ?? "BackendUsers",
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
                            ClockSkew = TimeSpan.Zero
                        };
                    });

                    services.AddAuthorization();

                    // ===== Controllers =====
                    services.AddControllers()
                        .AddApplicationPart(typeof(HostBuilderConfig).Assembly)
                        .AddControllersAsServices()
                        .ConfigureApiBehaviorOptions(options =>
                        {
                            options.InvalidModelStateResponseFactory = context =>
                            {
                                var errors = context.ModelState
                                    .Where(e => e.Value?.Errors.Count > 0)
                                    .ToDictionary(
                                        e => e.Key,
                                        e => e.Value?.Errors.Select(x => x.ErrorMessage).ToArray()
                                    );

                                return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new
                                {
                                    Message = "Validation failed",
                                    Errors = errors
                                });
                            };
                        });

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

                    // ===== Repository =====
                    services.AddScoped<IAdministratorRepository, AdministratorRepository>();
                    services.AddScoped<ICustomerRepository, CustomerRepository>();
                    services.AddScoped<IFileRepository, FileRepository>();

                    // ===== Service =====
                    services.AddScoped<IAdministratorService, AdministratorService>();
                    services.AddScoped<ICustomerService, CustomerService>();
                    services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
                    services.AddScoped<IJwtTokenService, JwtTokenService>();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.Configure(app =>
                    {
                        // Sử dụng ExceptionHandlingMiddleware thay vì UseExceptionHandler
                        app.UseExceptionHandlingMiddleware();

                        app.UseRouting();

                        // Middleware để log request
                        app.Use(async (context, next) =>
                        {
                            Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
                            await next();
                            Console.WriteLine($"Response: {context.Response.StatusCode}");
                        });

                        app.UseCors("AllowAll");
                        app.UseAuthentication();
                        app.UseAuthorization();

                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllers();
                            
                            // Log registered endpoints
                            var endpointDataSource = endpoints.DataSources.FirstOrDefault();
                            if (endpointDataSource != null)
                            {
                                Console.WriteLine("========== Registered Endpoints ==========");
                                var endpointList = endpointDataSource.Endpoints.ToList();
                                
                                if (endpointList.Count == 0)
                                {
                                    Console.WriteLine("⚠️  NO ENDPOINTS FOUND!");
                                    Console.WriteLine("⚠️  Controllers may not be discovered.");
                                    Console.WriteLine($"⚠️  Assembly: {typeof(HostBuilderConfig).Assembly.FullName}");
                                }
                                else
                                {
                                    foreach (var endpoint in endpointList)
                                    {
                                        if (endpoint is Microsoft.AspNetCore.Routing.RouteEndpoint routeEndpoint)
                                        {
                                            Console.WriteLine($"  [{routeEndpoint.RoutePattern.RawText}] {endpoint.DisplayName}");
                                        }
                                        else
                                        {
                                            Console.WriteLine($"  {endpoint.DisplayName}");
                                        }
                                    }
                                }
                                Console.WriteLine("==========================================");
                            }
                            else
                            {
                                Console.WriteLine("⚠️  WARNING: No endpoint data source found!");
                            }
                        });
                    });
                });
    }
}