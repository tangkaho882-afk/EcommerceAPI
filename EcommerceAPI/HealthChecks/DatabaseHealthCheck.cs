using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using EcommerceAPI.Data;

namespace EcommerceAPI.HealthChecks
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly AppDbContext _dbContext;

        public DatabaseHealthCheck(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var canConnect = await _dbContext.Database
                    .CanConnectAsync(cancellationToken);

                if (canConnect)
                {
                    return HealthCheckResult.Healthy(
                        "Database connection is healthy.");
                }

                return HealthCheckResult.Unhealthy(
                    "Database connection is unavailable.");
            }
            catch(Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    "Database health check failed.",ex);
            }
        }
    }
}