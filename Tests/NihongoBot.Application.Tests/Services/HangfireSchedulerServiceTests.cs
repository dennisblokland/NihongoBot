using AutoFixture;

using Hangfire;
using Hangfire.Storage;

using Microsoft.Extensions.Logging;

using Moq;

using NihongoBot.Application.Services;
using NihongoBot.Domain.Entities;
using NihongoBot.Domain.Interfaces.Repositories;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Requests;
using Telegram.Bot.Exceptions;
using User = NihongoBot.Domain.User;
using Hangfire.Common;
using NihongoBot.Application.Interfaces;
using NihongoBot.Application.Models;

namespace NihongoBot.Application.Tests.Services;

public class HangfireSchedulerServiceTest
{
	private readonly Mock<IUserRepository> _userRepositoryMock = new();
	private readonly Mock<IQuestionRepository> _questionRepositoryMock = new();
	private readonly Mock<IRecurringJobManager> _recurringJobManagerMock = new();
	private readonly Mock<ITelegramBotClient> _botClientMock = new();
	private readonly Mock<ILogger<HangfireSchedulerService>> _loggerMock = new();
	private readonly Mock<JobStorage> _jobStorage = new();
	private readonly Mock<IJlptVocabApiService> _jlptVocabApiService = new();
	private readonly HangfireSchedulerService _hangfireSchedulerService;
	private readonly Fixture _fixture = new();

	public HangfireSchedulerServiceTest()
	{
		_hangfireSchedulerService = new HangfireSchedulerService(
			_userRepositoryMock.Object,
			_questionRepositoryMock.Object,
			_botClientMock.Object,
			_recurringJobManagerMock.Object,
			_loggerMock.Object,
			_jobStorage.Object,
			_jlptVocabApiService.Object
		);
	}

	[Fact]
	public async Task InitializeSchedulerAsync_ShouldScheduleJobs()
	{
		// Act
		await _hangfireSchedulerService.InitializeSchedulerAsync();

		// Assert
		_recurringJobManagerMock.Verify(manager => manager.AddOrUpdate(
			"ScheduleHiraganaJobs",
			It.IsAny<Job>(),
			It.IsAny<string>(),
			It.IsAny<RecurringJobOptions>()), Times.Once);


		_recurringJobManagerMock.Verify(manager => manager.AddOrUpdate(
			"CheckExpiredQuestions",
			It.IsAny<Job>(),
			It.IsAny<string>(),
			It.IsAny<RecurringJobOptions>()), Times.Once);

		_loggerMock.Verify(logger => logger.Log(
			LogLevel.Information,
			It.IsAny<EventId>(),
			It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Hiragana jobs scheduled successfully.")),
			null,
			It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
	}

	[Theory]
	[InlineData(1)]
	[InlineData(2)]
	[InlineData(3)]
	[InlineData(4)]
	[InlineData(5)]
	[InlineData(6)]
	public async Task ScheduleHiraganaJobs_ShouldScheduleJobsForAllUsers_WithDifferentQuestionsPerDay(int questionsPerDay)
	{
		// Arrange
		List<User> users = _fixture.Build<User>()
			.With(u => u.QuestionsPerDay, questionsPerDay)
			.CreateMany(2)
			.ToList();

		_userRepositoryMock
			.Setup(repo => repo.GetAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(users);

		// Act
		await _hangfireSchedulerService.ScheduleHiraganaJobs();

		// Assert
		foreach (User? user in users)
		{
			_recurringJobManagerMock.Verify(manager => manager.AddOrUpdate(
				It.Is<string>(jobId => jobId.Contains($"SendHiragana_") && jobId.Contains(user.Id.ToString())),
				It.IsAny<Job>(),
				It.IsAny<string>(),
				It.IsAny<RecurringJobOptions>()), Times.Exactly(questionsPerDay));
		}
	}

	[Fact]
	public async Task CheckExpiredQuestions_ShouldMarkQuestionsAsExpiredAndNotifyUsers()
	{
		// Arrange
		List<Question> expiredQuestions =
		[
			_fixture.Create<Question>(),
		];

		User user = _fixture.Build<User>().With(x => x.Id, expiredQuestions[0].UserId).Create();

		_questionRepositoryMock
			.Setup(repo => repo.GetExpiredQuestionsAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(expiredQuestions);

		_userRepositoryMock
			.Setup(repo => repo.FindByIdAsync(user.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(user);

		// Act
		await _hangfireSchedulerService.CheckExpiredQuestions(CancellationToken.None);

		// Assert
		foreach (Question question in expiredQuestions)
		{
			Assert.True(question.IsExpired);
		}

		_botClientMock
			.Setup(client => client.SendRequest(
				It.Is<SendMessageRequest>(request => request.Text.Contains("The correct answer was a")),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(new Message());

		_questionRepositoryMock.Verify(context => context.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
	}

	[Fact]
	public async Task CheckExpiredQuestions_ShouldRemoveBlockedUser_WhenBotIsBlocked()
	{
		// Arrange
		List<Question> expiredQuestions =
		[
			_fixture.Create<Question>(),
		];

		User user = _fixture.Build<User>().With(x => x.Id, expiredQuestions[0].UserId).Create();

		_questionRepositoryMock
			.Setup(repo => repo.GetExpiredQuestionsAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(expiredQuestions);

		_userRepositoryMock
			.Setup(repo => repo.FindByIdAsync(user.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(user);

		_botClientMock
			.Setup(client => client.SendRequest(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new ApiRequestException("Forbidden: bot was blocked by the user", 403));

		// Act
		await _hangfireSchedulerService.CheckExpiredQuestions(CancellationToken.None);

		// Assert
		_userRepositoryMock.Verify(repo => repo.Remove(user), Times.Once);
		_userRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
		_loggerMock.Verify(logger => logger.Log(
			LogLevel.Information,
			It.IsAny<EventId>(),
			It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("blocked the bot")),
			null,
			It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
	}

	[Fact]
	public async Task CheckExpiredQuestions_ShouldRemoveBlockedUser_WhenBotIsBlockedOnPendingAcceptance()
	{
		// Arrange
		List<Question> unansweredQuestions =
		[
			_fixture.Create<Question>(),
		];

		User user = _fixture.Build<User>().With(x => x.Id, unansweredQuestions[0].UserId).Create();

		_questionRepositoryMock
			.Setup(repo => repo.GetExpiredQuestionsAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<Question>());

		_questionRepositoryMock
			.Setup(repo => repo.GetExpiredPendingAcceptanceQuestionsAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(unansweredQuestions);

		_userRepositoryMock
			.Setup(repo => repo.FindByIdAsync(user.Id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(user);

		_botClientMock
			.Setup(client => client.SendRequest(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new ApiRequestException("Forbidden: bot was blocked by the user", 403));

		// Act
		await _hangfireSchedulerService.CheckExpiredQuestions(CancellationToken.None);

		// Assert
		_userRepositoryMock.Verify(repo => repo.Remove(user), Times.Once);
		_userRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
		_loggerMock.Verify(logger => logger.Log(
			LogLevel.Information,
			It.IsAny<EventId>(),
			It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("blocked the bot")),
			null,
			It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
	}

	[Fact]
	public async Task SendWordOfTheDay_ShouldRemoveBlockedUser_WhenBotIsBlocked()
	{
		// Arrange
		List<User> users =
		[
			_fixture.Build<User>().With(u => u.WordOfTheDayEnabled, true).Create(),
			_fixture.Build<User>().With(u => u.WordOfTheDayEnabled, true).Create()
		];

		var jlptWord = _fixture.Create<JLPTWord>();

		_userRepositoryMock
			.Setup(repo => repo.GetAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(users);

		_jlptVocabApiService
			.Setup(service => service.GetRandomWordAsync(It.IsAny<int>()))
			.ReturnsAsync(jlptWord);

		// First user: bot blocked, second user: success
		_botClientMock
			.SetupSequence(client => client.SendRequest(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new ApiRequestException("Forbidden: bot was blocked by the user", 403))
			.ReturnsAsync(new Message());

		// Act
		await _hangfireSchedulerService.SendWordOfTheDay(CancellationToken.None);

		// Assert
		_userRepositoryMock.Verify(repo => repo.Remove(users[0]), Times.Once);
		_userRepositoryMock.Verify(repo => repo.Remove(users[1]), Times.Never);
		_userRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
		_loggerMock.Verify(logger => logger.Log(
			LogLevel.Information,
			It.IsAny<EventId>(),
			It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("blocked the bot")),
			null,
			It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
	}

	[Fact]
	public async Task SendWordOfTheDay_ShouldContinueProcessing_WhenOneUserBlocks()
	{
		// Arrange
		List<User> users =
		[
			_fixture.Build<User>().With(u => u.WordOfTheDayEnabled, true).Create(),
			_fixture.Build<User>().With(u => u.WordOfTheDayEnabled, true).Create()
		];

		var jlptWord = _fixture.Create<JLPTWord>();

		_userRepositoryMock
			.Setup(repo => repo.GetAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(users);

		_jlptVocabApiService
			.Setup(service => service.GetRandomWordAsync(It.IsAny<int>()))
			.ReturnsAsync(jlptWord);

		// First user: bot blocked, second user: success
		_botClientMock
			.SetupSequence(client => client.SendRequest(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new ApiRequestException("Forbidden: bot was blocked by the user", 403))
			.ReturnsAsync(new Message());

		// Act
		await _hangfireSchedulerService.SendWordOfTheDay(CancellationToken.None);

		// Assert - Both messages should be attempted (first throws, second succeeds)
		_botClientMock.Verify(client => client.SendRequest(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
	}

	[Fact]
	public void ScheduleHiraganaJobsForUser_ShouldAddOrUpdateJobs()
	{
		// Arrange
		User user = _fixture.Build<User>()
			.With(u => u.QuestionsPerDay, 2)
			.Create();
		Mock<IStorageConnection> jobStorageMock = new Mock<IStorageConnection>();
		JobStorage.Current = Mock.Of<JobStorage>(storage => storage.GetConnection() == jobStorageMock.Object);

		// Act
		_hangfireSchedulerService.ScheduleHiraganaJobsForUser(user);

		// Assert
		_recurringJobManagerMock.Verify(manager => manager.AddOrUpdate(
			It.Is<string>(jobId => jobId.Contains($"SendHiragana_") && jobId.Contains(user.Id.ToString())),
			It.IsAny<Job>(),
			It.IsAny<string>(),
			It.IsAny<RecurringJobOptions>()), Times.AtLeastOnce);

	}
}
