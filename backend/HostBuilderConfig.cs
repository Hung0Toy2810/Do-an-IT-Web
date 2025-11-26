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
using Backend.Middleware;
using Backend.Service.CategoryService;
using Backend.Repository.CategoryRepository;
using Backend.Service.Product;
using Backend.Repository.Product;
using Backend.Service.Inventory;
using Backend.Repository;
using Backend.Repository.CommentRepository;
using Backend.Service.CommentService;
using Backend.Repository.CartRepository;
using Backend.Service.Cart;
using Backend.Repository.VNPayPaymentRepository;
using Backend.Repository.InvoiceStatusHistoryRepository;
using Backend.Repository.InvoiceRepository;
using Backend.Repository.InvoiceDetailRepository;
using Backend.Service.Payment;
using Backend.Service.ViettelPost;
using Backend.Repository.ViettelPost;
using Backend.Service.Stock;
using Backend.Service.Shipping;
using Backend.Service;
using Backend.Service.Checkout;
using Backend.Services;
namespace Backend 
{
    public static class HostBuilderConfig
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHttpContextAccessor();
                    var configuration = hostContext.Configuration;
                    // AddHttpClient()
                    services.AddHttpClient();

                    // ===== Logging =====
                    services.AddLogging(builder =>
                    {
                        builder.AddConsole();
                        builder.SetMinimumLevel(LogLevel.Debug);
                    });

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
                        redisConfig.ConnectTimeout = 5000;
                        redisConfig.SyncTimeout = 5000;

                        var redis = ConnectionMultiplexer.Connect(redisConfig);
                        services.AddSingleton<IConnectionMultiplexer>(redis);
                        // Test kết nối và log
                        var loggerFactory = hostContext.HostingEnvironment.IsDevelopment()
                            ? services.BuildServiceProvider().GetService<ILoggerFactory>()
                            : null;
                        var logger = loggerFactory?.CreateLogger("Redis");
                        logger?.LogInformation("Successfully connected to Redis at {Host}:{Port}", configuration["Redis:Host"], configuration["Redis:Port"]);
                        var db = redis.GetDatabase();
                        db.PingAsync().GetAwaiter().GetResult();
                        logger?.LogInformation("Redis ping successful");
                    }
                    catch (RedisConnectionException ex)
                    {
                        var loggerFactory = hostContext.HostingEnvironment.IsDevelopment()
                            ? services.BuildServiceProvider().GetService<ILoggerFactory>()
                            : null;
                        var logger = loggerFactory?.CreateLogger("Redis");
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
                    services.AddJwtAuthentication(configuration);

                    services.AddAuthorization();
                    
                    // ===== Viettel Post Token Service (luôn có token) =====
                    services.AddSingleton<ViettelPostTokenService>();
                    services.AddHostedService<ViettelPostBackgroundService>();
                    services.AddSingleton<ViettelPostTokenService>();
                    services.AddHostedService<ViettelPostBackgroundService>();
                    services.AddScoped<IViettelPostAddressRepository, ViettelPostAddressRepository>();
                    services.AddScoped<IViettelPostAddressService, ViettelPostAddressService>();
                    //services.AddHostedService<ViettelPostAddressInitializer>();  ##### tạm thời tắt đi để build nhanh
                    services.AddHostedService<ViettelPostAddressMonthlyUpdater>();
                    services.AddScoped<IViettelPostMockService, ViettelPostMockService>();
                    // ===== Controllers =====
                    services.AddControllers()
                        .AddApplicationPart(typeof(HostBuilderConfig).Assembly)
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

                    // ===== Repository và Service =====
                    services.AddScoped<IAdministratorRepository, AdministratorRepository>();
                    services.AddScoped<ICustomerRepository, CustomerRepository>();
                    services.AddScoped<IFileRepository, FileRepository>();
                    services.AddScoped<IAdministratorService, AdministratorService>();
                    services.AddScoped<ICustomerService, CustomerService>();
                    services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
                    services.AddScoped<IJwtTokenService, JwtTokenService>();
                    services.AddScoped<ICategoryRepository, CategoryRepository>();
                    services.AddScoped<ICategoryService, CategoryService>();
                    services.AddScoped<IProductDocumentRepository, ProductDocumentRepository>();
                    services.AddScoped<IProductSearchRepository, ProductSearchRepository>();
                    services.AddScoped<IProductStockRepository, ProductStockRepository>();
                    services.AddScoped<IStockReservationRepository, StockReservationRepository>();
                    services.AddScoped<IProductRepository, ProductRepository>();
                    services.AddScoped<IProductDocumentService, ProductDocumentService>();
                    services.AddScoped<IProductSearchService, ProductSearchService>();
                    services.AddScoped<IProductStockService, ProductStockService>();
                    services.AddScoped<IProductService, ProductService>();
                    services.AddScoped<IInventoryService, InventoryService>();
                    services.AddScoped<IShipmentBatchRepository, ShipmentBatchRepository>();
                    services.AddScoped<ICommentRepository, CommentRepository>();
                    services.AddScoped<ICommentService, CommentService>();
                    services.AddScoped<ICartRepository, CartRepository>();
                    services.AddScoped<ICartService, CartService>();
                    services.AddScoped<IVNPayPaymentRepository, VNPayPaymentRepository>();
                    services.AddScoped<IInvoiceStatusHistoryRepository, InvoiceStatusHistoryRepository>();
                    services.AddScoped<IInvoiceRepository, InvoiceRepository>();
                    services.AddScoped<IInvoiceDetailRepository, InvoiceDetailRepository>();
                    services.AddScoped<IVNPayService, VNPayService>();
                    services.AddScoped<IViettelPostAddressValidatorService, ViettelPostAddressValidatorService>();
                    services.AddScoped<IStockAllocationService, StockAllocationService>();
                    services.AddScoped<IViettelPostMockService, ViettelPostMockService>();
                    services.AddScoped<IShippingService, ShippingService>();
                    // StockCleanupService : BackgroundService
                    services.AddHostedService<StockCleanupService>();
                    services.AddScoped<ICheckoutService, CheckoutService>();
                    services.AddScoped<IRedisProductViewService, RedisProductViewService>();
                    services.AddScoped<BestSellerService>();

                    //ShippingSimulationService : BackgroundService
                    services.AddHostedService<ShippingSimulationService>();

                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.Configure(app =>
                    {
                        app.UseExceptionHandlingMiddleware();
                        app.UseRouting();

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
                            var endpointDataSource = endpoints.DataSources.FirstOrDefault();
                            if (endpointDataSource != null)
                            {
                                Console.WriteLine("========== Registered Endpoints ==========");
                                var endpointList = endpointDataSource.Endpoints.ToList();
                                if (endpointList.Count == 0)
                                {
                                    Console.WriteLine("NO ENDPOINTS FOUND!");
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
                        });
                    });
                });
    }
}