using AutoFixture;

using Hangfire;
using Hangfire.Storage;

using Microsoft.Extensions.Logging;

using Moq;

using NihongoBot.Application.Services;
using NihongoBot.Domain.Entities;
using NihongoBot.Domain.Interfaces.Repositories;
using NihongoBot.Persistence;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Requests;
using User = NihongoBot.Domain.User;
using Hangfire.Common;

namespace NihongoBot.Application.Tests.Services;

public class HangfireSchedulerServiceTest
{
	private readonly Mock<IUserRepository> _userRepositoryMock = new();
	private readonly Mock<IQuestionRepository> _questionRepositoryMock = new();
	private readonly Mock<IRecurringJobManager> _recurringJobManagerMock = new();
	private readonly Mock<ITelegramBotClient> _botClientMock = new();
	private readonly Mock<ILogger<HangfireSchedulerService>> _loggerMock = new();
	private readonly Mock<AppDbContext> _dbContextMock = new();
	private readonly Mock<JobStorage> _jobStorage = new();
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
			_jobStorage.Object
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

	[Fact]
	public async Task ScheduleHiraganaJobs_ShouldScheduleJobsForAllUsers()
	{
		// Arrange
		List<User> users = _fixture.CreateMany<User>(2).ToList();

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
			It.IsAny<RecurringJobOptions>()), Times.AtLeastOnce);

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

		_questionRepositoryMock.Verify(context => context.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public void ScheduleHiraganaJobsForUser_ShouldAddOrUpdateJobs()
	{
		// Arrange
		User user = _fixture.Create<User>();
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
