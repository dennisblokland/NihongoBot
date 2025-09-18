using Microsoft.Extensions.Logging;

using Moq;

using NihongoBot.Application.Interfaces;
using NihongoBot.Application.Services;
using NihongoBot.Domain.Aggregates.Kana;
using NihongoBot.Domain.Entities;
using NihongoBot.Domain.Enums;
using NihongoBot.Domain.Interfaces.Repositories;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Requests;
using AutoFixture;

namespace NihongoBot.Application.Tests.Services;

public class HiraganaServiceTest
{
	private readonly Mock<ITelegramBotClient> _botClientMock = new();
	private readonly Mock<IQuestionRepository> _questionRepositoryMock = new();
	private readonly Mock<IKanaRepository> _kanaRepositoryMock = new();
	private readonly Mock<IUserRepository> _userRepositoryMock = new();
	private readonly Mock<IImageCacheService> _imageCacheServiceMock = new();
	private readonly Mock<IStrokeOrderService> _strokeOrderServiceMock = new();
	private readonly Mock<ILogger<HiraganaService>> _loggerMock = new();
	private readonly HiraganaService _hiraganaService;

	public HiraganaServiceTest()
	{
		// Setup image cache service to return a dummy image
		_imageCacheServiceMock
			.Setup(service => service.GetOrGenerateImageAsync(It.IsAny<string>()))
			.ReturnsAsync(new byte[] { 1, 2, 3, 4 }); // dummy image bytes

		// Setup stroke order service defaults
		_strokeOrderServiceMock
			.Setup(service => service.HasStrokeOrderAnimation(It.IsAny<string>()))
			.Returns(false); // Default to no stroke order available

		_hiraganaService = new HiraganaService(
			_questionRepositoryMock.Object,
			_kanaRepositoryMock.Object,
			_userRepositoryMock.Object,
			_botClientMock.Object,
			_imageCacheServiceMock.Object,
			_strokeOrderServiceMock.Object,
			_loggerMock.Object
		);
	}

	[Fact]
	public async Task SendHiraganaMessage_ShouldSendPhotoAndSaveQuestion_WhenKanaIsFound()
	{
		// Arrange
		long telegramId = 123456789L;
		Guid userId = Guid.NewGuid();
		CancellationToken cancellationToken = CancellationToken.None;

		Kana kana = new()
		{
			Character = "あ",
			Romaji = "a"
		};

		// Mock user with all characters enabled
		Domain.User user = new Domain.User(telegramId, "testuser");
		_userRepositoryMock
			.Setup(repo => repo.GetByTelegramIdAsync(telegramId, cancellationToken))
			.ReturnsAsync(user);

		Message message = new();
		// Note: MessageId is read-only and will be set by the Telegram API.

		_kanaRepositoryMock
			.Setup(repo => repo.GetRandomAsync(KanaType.Hiragana, It.IsAny<List<string>>(), cancellationToken))
			.ReturnsAsync(kana);

        _botClientMock
			.Setup(client => client.SendRequest(
				It.IsAny<SendPhotoRequest>(), 
				It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message());
		Fixture fixture = new();
		_questionRepositoryMock
			.Setup(repo => repo.AddAsync(It.IsAny<Question>(), cancellationToken))
			.ReturnsAsync(fixture.Create<Question>());

		// Act
		await _hiraganaService.SendHiraganaMessage(telegramId, userId, cancellationToken);

		// Assert
       _botClientMock.Verify(client => client.SendRequest(
            It.IsAny<SendMessageRequest>(), 
            It.IsAny<CancellationToken>()), Times.Once);

		_questionRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Question>(q =>
			q.UserId == userId &&
			q.QuestionType == QuestionType.Hiragana &&
			q.QuestionText == kana.Character &&
			q.CorrectAnswer == kana.Romaji &&
			q.MessageId == message.MessageId), cancellationToken), Times.Once);

		_questionRepositoryMock.Verify(repo => repo.SaveChangesAsync(cancellationToken), Times.Once);
	}

	[Fact]
	public async Task SendHiraganaMessage_ShouldLogWarning_WhenNoKanaIsFound()
	{
		// Arrange
		long telegramId = 123456789L;
		Guid userId = Guid.NewGuid();
		CancellationToken cancellationToken = CancellationToken.None;

		// Mock user with all characters enabled
		Domain.User user = new Domain.User(telegramId, "testuser");
		_userRepositoryMock
			.Setup(repo => repo.GetByTelegramIdAsync(telegramId, cancellationToken))
			.ReturnsAsync(user);

		_kanaRepositoryMock
			.Setup(repo => repo.GetRandomAsync(KanaType.Hiragana, It.IsAny<List<string>>(), cancellationToken))
			.ReturnsAsync((Kana?) null);

		// Act
		await _hiraganaService.SendHiraganaMessage(telegramId, userId, cancellationToken);

		// Assert
		_loggerMock.Verify(logger => logger.Log(
			LogLevel.Warning,
			It.IsAny<EventId>(),
			It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No Kana found in the database with enabled characters")),
			null,
			It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);

	
       _botClientMock.Verify(client => client.SendRequest(
            It.IsAny<SendPhotoRequest>(), 
            It.IsAny<CancellationToken>()), Times.Never);

		_questionRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Question>(), cancellationToken), Times.Never);
		_questionRepositoryMock.Verify(repo => repo.SaveChangesAsync(cancellationToken), Times.Never);
	}

	[Fact]
	public async Task SendMultipleChoiceHiraganaMessage_ShouldCreateQuestionWithOptions_WhenKanaIsFound()
	{
		// Arrange
		long telegramId = 123456789L;
		Guid userId = Guid.NewGuid();
		CancellationToken cancellationToken = CancellationToken.None;

		// Mock user with all characters enabled
		Domain.User user = new Domain.User(telegramId, "testuser");
		_userRepositoryMock
			.Setup(repo => repo.GetByTelegramIdAsync(telegramId, cancellationToken))
			.ReturnsAsync(user);

		Kana kana = new()
		{
			Character = "つ",
			Romaji = "tsu"
		};

		List<Kana> wrongAnswers = new List<Kana>
		{
			new() { Character = "か", Romaji = "ka" },
			new() { Character = "く", Romaji = "ku" },
			new() { Character = "こ", Romaji = "ko" }
		};

		_kanaRepositoryMock
			.Setup(repo => repo.GetRandomAsync(KanaType.Hiragana, It.IsAny<List<string>>(), cancellationToken))
			.ReturnsAsync(kana);

		_kanaRepositoryMock
			.Setup(repo => repo.GetWrongAnswersAsync(kana.Romaji, KanaType.Hiragana, It.IsAny<List<string>>(), 3, cancellationToken))
			.ReturnsAsync(wrongAnswers);

		_botClientMock
			.Setup(client => client.SendRequest(
				It.IsAny<SendPhotoRequest>(), 
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(new Message());

		_questionRepositoryMock
			.Setup(repo => repo.AddAsync(It.IsAny<Question>(), cancellationToken))
			.ReturnsAsync((Question q, CancellationToken ct) => q);

		// Act
		await _hiraganaService.SendMultipleChoiceHiraganaMessage(telegramId, userId, cancellationToken);

		// Assert
		_questionRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Question>(q =>
			q.UserId == userId &&
			q.QuestionType == QuestionType.MultipleChoiceHiragana &&
			q.QuestionText == kana.Character &&
			q.CorrectAnswer == kana.Romaji &&
			!string.IsNullOrEmpty(q.MultipleChoiceOptions)), cancellationToken), Times.Once);

		_questionRepositoryMock.Verify(repo => repo.SaveChangesAsync(cancellationToken), Times.Exactly(2));
	}
}
