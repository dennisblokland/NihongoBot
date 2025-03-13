using Hangfire;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NihongoBot.Application.Helpers;
using NihongoBot.Persistence;

namespace NihongoBot.Application.Services;

public class HangfireSchedulerService : IHostedService
{
    private readonly ILogger<HangfireSchedulerService> _logger;
	private readonly IRecurringJobManager _recurringJobManager;
	private readonly AppDbContext _dbContext;

	public HangfireSchedulerService(
        ILogger<HangfireSchedulerService> logger,
         IRecurringJobManager recurringJobManager,
         IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
		_recurringJobManager = recurringJobManager;
		_dbContext = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
	}

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Hangfire job scheduler...");

	    ScheduleHiraganaJobs();

        //schedule a job to schedule the jobs again at midnight
        _recurringJobManager.AddOrUpdate(
            "ScheduleHiraganaJobs",
            () => ScheduleHiraganaJobs(),
            Cron.Daily(0, 0)
        );

        _logger.LogInformation("Hiragana jobs scheduled successfully.");
        return Task.CompletedTask;
    }

	public void ScheduleHiraganaJobs()
	{
		List<Domain.User> users = _dbContext.Users.ToList();
        int jobCount = 2; // This can be replaced with a configuration value in the future

        foreach (Domain.User user in users)
        {
            List<DateTimeOffset> scheduledTimes = [];

            for (int i = 0; i < jobCount; i++)
            {
            int hour;
            if (i == 0)
            {
                hour = Random.Shared.Next(9, 21); // First job between 9 and 18 inclusive
            }
            else
            {
                hour = Random.Shared.Next(scheduledTimes[i - 1].Hour + 2, 21); // At least 2 hours after the previous job and before 21
            }

            int minute = Random.Shared.Next(0, 4) * 15; // Whole 15 minutes (0, 15, 30, 45)
            DateTimeOffset scheduledTime = DateTimeOffset.Now.Date.AddHours(hour).AddMinutes(minute);
            if (scheduledTime < DateTimeOffset.Now)
            {
                scheduledTime = scheduledTime.AddDays(1);
            }
          
            scheduledTimes.Add(scheduledTime);

            string jobId = $"SendHiragana_{i + 1}_{user.Id}";

            _recurringJobManager.AddOrUpdate<HiraganaService>(
                jobId,
                service => service.SendHiraganaMessage(user.TelegramId),
                Cron.Yearly(scheduledTime.Month, scheduledTime.Day, scheduledTime.Hour, scheduledTime.Minute)
            );
            }
        }
	}

	public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Hangfire Scheduler stopping...");
        return Task.CompletedTask;
    }
}
