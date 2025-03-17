using Hangfire;
using Hangfire.Storage;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NihongoBot.Application.Helpers;
using NihongoBot.Domain;
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

		// Schedule a job to check for expired questions every minute
		_recurringJobManager.AddOrUpdate(
			"CheckExpiredQuestions",
			() => CheckExpiredQuestions(),
			Cron.Minutely
		);

		_logger.LogInformation("Hiragana jobs scheduled successfully.");
		return Task.CompletedTask;
	}

	public void ScheduleHiraganaJobs()
	{
		List<User> users = _dbContext.Users.ToList();

		foreach (User user in users)
		{
			ScheduleHiraganaJobsForUser(user);
		}
	}

	public void ScheduleHiraganaJobsForUser(User user)
	{
		List<RecurringJobDto> currentJobs = [];
		IStorageConnection conn = JobStorage.Current.GetConnection();
		int jobCount = 2; // This can be replaced with a configuration value in the future
		JobStorageConnection storage = (JobStorageConnection) conn;
		if (storage != null)
		{
			currentJobs = storage.GetRecurringJobs().Where(j => j.Id.Contains("SendHiragana_") && j.Id.Contains(user.Id.ToString())).ToList();
			if (currentJobs.Count > jobCount)
			{
				// Remove all jobs
				foreach (RecurringJobDto job in currentJobs)
				{
					_recurringJobManager.RemoveIfExists(job.Id);

				}
				currentJobs = [];
			}
		}

		//TODO: Get the user's timezone from the database
		//TODO: make the start and end times configurable
		TimeOnly start = new(9, 0);  // Start time (09:00)
		TimeOnly end = new(21, 0);   // End time (21:00)

		List<TimeOnly> scheduledTimes = TimeGenerator.GetRandomTimes(start, end, jobCount);

		for (int i = 0; i < jobCount; i++)
		{
			string jobId = $"SendHiragana_{i + 1}_{user.Id}";

			if (currentJobs.Any(j => j.Id == jobId && j.NextExecution > DateTime.UtcNow  && j.NextExecution.HasValue && j.NextExecution.Value < DateTime.UtcNow.AddHours(24)))
			{
				continue;
			}

			TimeOnly scheduledTime = scheduledTimes[i];

			DateTimeOffset scheduledDateTime = new(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, scheduledTime.Hour, scheduledTime.Minute, 0, 0, TimeSpan.Zero);

			if (scheduledDateTime < DateTimeOffset.Now)
			{
				scheduledDateTime = scheduledDateTime.AddDays(1);
			}

			_recurringJobManager.AddOrUpdate<HiraganaService>(
				jobId,
				service => service.SendHiraganaMessage(user.TelegramId, user.Id),
				Cron.Yearly(scheduledDateTime.Month, scheduledDateTime.Day, scheduledDateTime.Hour, scheduledDateTime.Minute)
			);
		}

	}

	public async Task CheckExpiredQuestions()
	{
		DateTime now = DateTime.UtcNow;
		List<Question> expiredQuestions = _dbContext.Questions
			.Where(q => !q.IsAnswered && !q.IsExpired && q.SentAt.AddMinutes(q.TimeLimit) <= now)
			.ToList();

		foreach (Question question in expiredQuestions)
		{
			question.IsExpired = true;
			Domain.User? user = _dbContext.Users.FirstOrDefault(u => u.Id == question.UserId);
			if (user != null)
			{
				user.ResetStreak();
				await _botClient.SendMessage(user.TelegramId, $"Time's up! The correct answer was {question.CorrectAnswer}.");
			}
		}

		await _dbContext.SaveChangesAsync();
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Hangfire Scheduler stopping...");
		return Task.CompletedTask;
	}
}
