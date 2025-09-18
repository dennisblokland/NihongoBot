using Microsoft.Extensions.Logging;
using Moq;

using NihongoBot.Application.Handlers;
using NihongoBot.Application.Interfaces;
using NihongoBot.Application.Models;
using NihongoBot.Application.Services;
using NihongoBot.Domain.Entities;
using NihongoBot.Domain.Enums;
using NihongoBot.Domain.Interfaces.Repositories;

using Telegram.Bot;

namespace NihongoBot.Application.Tests.Handlers;

public class MultipleChoiceAnswerCallbackHandlerTest
{
	private readonly Mock<ITelegramBotClient> _botClientMock = new();
	private readonly Mock<IQuestionRepository> _questionRepositoryMock = new();
	private readonly Mock<IUserRepository> _userRepositoryMock = new();
	private readonly Mock<IKanaRepository> _kanaRepositoryMock = new();
	private readonly HiraganaService _hiraganaService;
	private readonly MultipleChoiceAnswerCallbackHandler _handler;

	public MultipleChoiceAnswerCallbackHandlerTest()
	{
		// Create mocks for HiraganaService dependencies
		var imageCacheServiceMock = new Mock<IImageCacheService>();
		var strokeOrderServiceMock = new Mock<IStrokeOrderService>();
		var loggerMock = new Mock<ILogger<HiraganaService>>();

		_hiraganaService = new HiraganaService(
			_questionRepositoryMock.Object,
			_kanaRepositoryMock.Object,
			_userRepositoryMock.Object,
			_botClientMock.Object,
			imageCacheServiceMock.Object,
			strokeOrderServiceMock.Object,
			loggerMock.Object
		);

		_handler = new MultipleChoiceAnswerCallbackHandler(
			_botClientMock.Object,
			_questionRepositoryMock.Object,
			_userRepositoryMock.Object,
			_kanaRepositoryMock.Object,
			_hiraganaService
		);
	}

	[Fact]
	public async Task HandleAsync_ShouldMarkQuestionAsAnswered_WhenAnswerIsCorrect()
	{
		// Arrange
		long chatId = 123456789L;
		Guid questionId = Guid.NewGuid();
		Guid userId = Guid.NewGuid();
		
		MultipleChoiceAnswerCallbackData callbackData = new MultipleChoiceAnswerCallbackData
		{
			QuestionId = questionId,
			SelectedAnswer = "tsu"
		};

		Question question = new Question
		{
			Id = questionId,
			UserId = userId,
			QuestionText = "つ",
			CorrectAnswer = "tsu",
			QuestionType = QuestionType.MultipleChoiceHiragana,
			IsAnswered = false,
			IsExpired = false,
			Attempts = 0
		};

		Domain.User user = new Domain.User(123456789L, "testuser");

		_questionRepositoryMock
			.Setup(repo => repo.FindByIdAsync(questionId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(question);

		_userRepositoryMock
			.Setup(repo => repo.FindByIdAsync(userId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(user);

		_kanaRepositoryMock
			.Setup(repo => repo.GetByCharacterAsync("つ", It.IsAny<CancellationToken>()))
			.ReturnsAsync(new NihongoBot.Domain.Aggregates.Kana.Kana { Character = "つ", Romaji = "tsu" });

		// Act
		await _handler.HandleAsync(chatId, callbackData, CancellationToken.None);

		// Assert
		Assert.True(question.IsAnswered);
		_userRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
		// Note: We can't easily verify HiraganaService method calls since it's a concrete class,
		// but the stroke order animation call will be made when the test runs
	}

	[Fact]
	public async Task HandleAsync_ShouldIncreaseAttempts_WhenAnswerIsIncorrect()
	{
		// Arrange
		long chatId = 123456789L;
		Guid questionId = Guid.NewGuid();
		Guid userId = Guid.NewGuid();
		
		MultipleChoiceAnswerCallbackData callbackData = new MultipleChoiceAnswerCallbackData
		{
			QuestionId = questionId,
			SelectedAnswer = "ka"  // Wrong answer
		};

		Question question = new Question
		{
			Id = questionId,
			UserId = userId,
			QuestionText = "つ",
			CorrectAnswer = "tsu",
			QuestionType = QuestionType.MultipleChoiceHiragana,
			IsAnswered = false,
			IsExpired = false,
			Attempts = 0
		};

		Domain.User user = new Domain.User(123456789L, "testuser");

		_questionRepositoryMock
			.Setup(repo => repo.FindByIdAsync(questionId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(question);

		_userRepositoryMock
			.Setup(repo => repo.FindByIdAsync(userId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(user);

		// Act
		await _handler.HandleAsync(chatId, callbackData, CancellationToken.None);

		// Assert
		Assert.Equal(1, question.Attempts);
		Assert.False(question.IsAnswered);
		Assert.False(question.IsExpired);
		_questionRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task HandleAsync_ShouldExpireQuestion_WhenMaxAttemptsReached()
	{
		// Arrange
		long chatId = 123456789L;
		Guid questionId = Guid.NewGuid();
		Guid userId = Guid.NewGuid();
		
		MultipleChoiceAnswerCallbackData callbackData = new MultipleChoiceAnswerCallbackData
		{
			QuestionId = questionId,
			SelectedAnswer = "ka"  // Wrong answer
		};

		Question question = new Question
		{
			Id = questionId,
			UserId = userId,
			QuestionText = "つ",
			CorrectAnswer = "tsu",
			QuestionType = QuestionType.MultipleChoiceHiragana,
			IsAnswered = false,
			IsExpired = false,
			Attempts = 2  // Already at 2 attempts, this will be the 3rd
		};

		Domain.User user = new Domain.User(123456789L, "testuser");

		_questionRepositoryMock
			.Setup(repo => repo.FindByIdAsync(questionId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(question);

		_userRepositoryMock
			.Setup(repo => repo.FindByIdAsync(userId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(user);

		// Act
		await _handler.HandleAsync(chatId, callbackData, CancellationToken.None);

		// Assert
		Assert.Equal(3, question.Attempts);
		Assert.False(question.IsAnswered);
		Assert.True(question.IsExpired);
		_userRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}