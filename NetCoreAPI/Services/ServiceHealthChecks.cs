using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NetCoreAPI.Services
{
    public class APIHealthChecks : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var ApiHealthy = true;

            //logic to determine if the API service is healthy

            if (ApiHealthy)
            {
                return Task.FromResult(HealthCheckResult.Healthy("API healthy."));
            }

            return Task.FromResult( new HealthCheckResult(context.Registration.FailureStatus, "API unhealthy."));
        }
    }

    public class DatabaseHealthChecks : IHealthCheck
    {
        public int Times { get; set; }
        public int Threshold { get; set; }
        public DatabaseHealthChecks(int times, int threshold)
        {
            Times = times;
            Threshold = threshold;
        }
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var DbHealthy = true;

            //logic to determine if the database is healthy

            if (DbHealthy)
            {
                return Task.FromResult(HealthCheckResult.Healthy("Database healthy."));
            }

            return Task.FromResult( new HealthCheckResult(context.Registration.FailureStatus, "Database unhealthy."));
        }
    }


}
