using Hangfire;
using Hangfire.Storage;

using Microsoft.Extensions.Logging;

using NihongoBot.Application.Helpers;
using NihongoBot.Application.Interfaces;
using NihongoBot.Application.Models;
using NihongoBot.Domain;
using NihongoBot.Domain.Entities;
using NihongoBot.Domain.Interfaces.Repositories;

using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace NihongoBot.Application.Services;

public class HangfireSchedulerService
{
	private readonly ILogger<HangfireSchedulerService> _logger;
	private readonly IUserRepository _userRepository;
	private readonly IQuestionRepository _questionRepository;
	private readonly IRecurringJobManager _recurringJobManager;
	private readonly ITelegramBotClient _botClient;
	private readonly JobStorage _jobStorage;
	private readonly IJlptVocabApiService _jlptVocabApiService;

	public HangfireSchedulerService(
		IUserRepository userRepository,
		IQuestionRepository questionRepository,
		ITelegramBotClient botClient,
		IRecurringJobManager recurringJobManager,
		ILogger<HangfireSchedulerService> logger,
		JobStorage jobStorage,
		IJlptVocabApiService jlptVocabApiService)
	{
		_logger = logger;
		_botClient = botClient;
		_userRepository = userRepository;
		_questionRepository = questionRepository;
		_recurringJobManager = recurringJobManager;
		_jobStorage = jobStorage;
		_jlptVocabApiService = jlptVocabApiService;
	}

	public async Task InitializeSchedulerAsync()
	{
		_logger.LogInformation("Starting Hangfire job scheduler...");

		await ScheduleHiraganaJobs();

		//schedule a job to schedule the jobs again at midnight
		_recurringJobManager.AddOrUpdate(
			"ScheduleHiraganaJobs",
			() => ScheduleHiraganaJobs(),
			Cron.Daily(0, 0)
		);

		// Schedule a job to check for expired questions every minute
		_recurringJobManager.AddOrUpdate(
			"CheckExpiredQuestions",
			//Hangfire replaces the CancellationToken internally
			() => CheckExpiredQuestions(CancellationToken.None),
			Cron.Minutely
		);

		_recurringJobManager.AddOrUpdate(
			"SendWordOfTheDay",
			//Hangfire replaces the CancellationToken internally
			() => SendWordOfTheDay(CancellationToken.None),
			// TODO: timezone of the user
			Cron.Daily(12, 0) // 12:00 PM UTC
		);

		_logger.LogInformation("Hiragana jobs scheduled successfully.");
	}

	public async Task SendWordOfTheDay(CancellationToken none)
	{
		JLPTWord? word = await _jlptVocabApiService.GetRandomWordAsync();
		if (word == null)
		{
			_logger.LogError("Failed to retrieve word of the day.");
			return;
		}
		IEnumerable<User> users = await _userRepository.GetAsync();
		foreach (User user in users)
		{
			await _botClient.SendMessage(user.TelegramId,
				$"<b>Word of the Day</b>\n\n" +
				$"<b>Word:</b> {word.Word}\n" +
				$"<b>Romaji:</b> {word.Romaji}\n" +
				$"<b>Meaning:</b> {word.Meaning}\n" +
				$"<b>Furigana:</b> {word.Furigana}",
				parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
				cancellationToken: none);
		}
	}

	public async Task ScheduleHiraganaJobs()
	{
		IEnumerable<User> users = await _userRepository.GetAsync();

		foreach (User user in users)
		{
			ScheduleHiraganaJobsForUser(user);
		}
	}

	public void ScheduleHiraganaJobsForUser(User user)
	{
		List<RecurringJobDto> currentJobs = [];
		IStorageConnection conn = _jobStorage.GetConnection();

		int jobCount = 2; // This can be replaced with a configuration value in the future
		if (conn != null)
		{
			currentJobs = conn.GetRecurringJobs().Where(j => j.Id.Contains("SendHiragana_") && j.Id.Contains(user.Id.ToString())).ToList();
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

			if (currentJobs.Any(j => j.Id == jobId && j.NextExecution > DateTime.UtcNow && j.NextExecution.HasValue && j.NextExecution.Value < DateTime.UtcNow.AddHours(24)))
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
				service => service.SendHiraganaMessage(user.TelegramId, user.Id, CancellationToken.None),
				Cron.Yearly(scheduledDateTime.Month, scheduledDateTime.Day, scheduledDateTime.Hour, scheduledDateTime.Minute)
			);
		}

	}

	public async Task CheckExpiredQuestions(CancellationToken cancellationToken)
	{
		IEnumerable<Question> expiredQuestions = await _questionRepository.GetExpiredQuestionsAsync(cancellationToken);

		foreach (Question question in expiredQuestions)
		{
			question.IsExpired = true;

			User? user = await _userRepository.FindByIdAsync(question.UserId, cancellationToken);
			if (user != null)
			{
				try
				{
					await _botClient.SendMessage(user.TelegramId,
					"You've reached the time limit. The correct answer was " + question.CorrectAnswer,
					replyParameters: question.MessageId, cancellationToken: cancellationToken);
				}
				catch (ApiRequestException ex) when (ex.Message.Contains("message to be replied not found"))
				{
					_logger.LogError(ex, "Failed to send message to user {UserId}: Message to be replied not found", user.Id);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Failed to send message to user {UserId}", user.Id);
					throw;
				}
				finally
				{
					user.ResetStreak();
					_userRepository.Update(user);
				}


			}
			_questionRepository.Update(question);
		}

		IEnumerable<Question> unansweredQuestions = await _questionRepository.GetExpiredPendingAcceptanceQuestionsAsync(cancellationToken);

		foreach (Question question in unansweredQuestions)
		{
			question.IsExpired = true;

			User? user = await _userRepository.FindByIdAsync(question.UserId, cancellationToken);
			if (user != null)
			{
				try
				{
					await _botClient.SendMessage(user.TelegramId,
						"You didn't confirm the challenge in time. Your streak has been reset",
						replyParameters: question.MessageId, cancellationToken: cancellationToken);
				}
				catch (ApiRequestException ex) when (ex.Message.Contains("message to be replied not found"))
				{
					_logger.LogError(ex, "Failed to send message to user {UserId}: Message to be replied not found", user.Id);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Failed to send message to user {UserId}", user.Id);
					throw;
				}
				finally
				{
					user.ResetStreak();
					_userRepository.Update(user);
				}
			}
			_questionRepository.Update(question);

		}


		await _questionRepository.SaveChangesAsync(cancellationToken);
	}
}
