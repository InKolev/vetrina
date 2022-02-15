using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vetrina.Server.Abstractions;
using Vetrina.Server.Jobs;

namespace Vetrina.Server.HostedServices
{
    public class InitializeHangfireJobs : IHostedService
    {
        private readonly IRecurringJobManager recurringJobManager;
        private readonly ILogger<InitializeHangfireJobs> logger;

        public InitializeHangfireJobs(
            IRecurringJobManager recurringJobManager,
            ILogger<InitializeHangfireJobs> logger)
        {
            this.recurringJobManager = recurringJobManager;
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Schedule recurring jobs.
            AddOrUpdateRecurringJob<IScrapeKauflandPromotionsJob>(
                CancellationToken.None,
                Cron.Weekly(DayOfWeek.Monday, 1, 0));

            AddOrUpdateRecurringJob<IScrapeLidlPromotionsJob>(
                CancellationToken.None,
                Cron.Weekly(DayOfWeek.Monday, 1, 15));

            AddOrUpdateRecurringJob<IScrapeBillaPromotionsJob>(
                CancellationToken.None,
                Cron.Weekly(DayOfWeek.Monday, 1, 30));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void AddOrUpdateRecurringJob<T>(
            CancellationToken cancellationToken,
            string cronExpression)
            where T : IJob
        {
            var jobName = typeof(T).Name;

            logger.LogInformation($"Registering {jobName} (recurring)...");

            recurringJobManager.AddOrUpdate<T>(
                recurringJobId: jobName,
                methodCall: job => job.Execute(cancellationToken),
                cronExpression: cronExpression,
                timeZone: TimeZoneInfo.Utc);

            logger.LogInformation($"Successfully registered {jobName}.");
        }
    }
}
