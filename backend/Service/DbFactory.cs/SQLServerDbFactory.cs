using Microsoft.Extensions.DependencyInjection;
using Backend.SQLDbContext;

namespace Backend.Service.DbFactory
{
    public interface IDbContextFactory
    {
        SQLServerDbContext CreateContext();
    }

    public class DbContextFactory : IDbContextFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public DbContextFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public SQLServerDbContext CreateContext()
        {
            return _serviceProvider.GetRequiredService<SQLServerDbContext>();
        }
    }
}