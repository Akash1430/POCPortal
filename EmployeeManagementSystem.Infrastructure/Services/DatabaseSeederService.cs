using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using EmployeeManagementSystem.Infrastructure.Context;
using EmployeeManagementSystem.Infrastructure.Data;

namespace EmployeeManagementSystem.Infrastructure.Services
{
    public class DatabaseSeederService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseSeederService> _logger;

        public DatabaseSeederService(IServiceProvider serviceProvider, ILogger<DatabaseSeederService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<EmployeeManagementSystemDbContext>();

            try
            {
                _logger.LogInformation("Starting database seeding...");
                await DataSeeder.SeedAsync(context);
                _logger.LogInformation("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
