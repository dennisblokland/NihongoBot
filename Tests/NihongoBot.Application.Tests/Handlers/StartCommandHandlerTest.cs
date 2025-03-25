using AutoFixture;

using Moq;

using NihongoBot.Application.Handlers;
using NihongoBot.Application.Tests.Extentions;
using NihongoBot.Domain.Interfaces.Repositories;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Requests;

namespace NihongoBot.Application.Tests.Handlers;

public class StartCommandHandlerTest
{
	private readonly Mock<ITelegramBotClient> _botClientMock = new();
	private readonly Mock<IUserRepository> _userRepositoryMock = new();
	private readonly StartCommandHandler _handler;
	private readonly Fixture _fixture = new();

	public StartCommandHandlerTest()
	{
		_handler = new StartCommandHandler(_botClientMock.Object, _userRepositoryMock.Object);
	}

	[Fact]
	public async Task HandleAsync_ShouldSendAlreadyRegisteredMessage_WhenUserExists()
	{
		// Arrange
		long chatId = 123456789L;
		string[] args = Array.Empty<string>();
		CancellationToken cancellationToken = CancellationToken.None;

		Domain.User existingUser = _fixture.Build<Domain.User>()
			.WithPrivate(u => u.TelegramId, chatId)
			.Create();

		_userRepositoryMock
			.Setup(repo => repo.GetByTelegramIdAsync(chatId, cancellationToken))
			.ReturnsAsync(existingUser);

		// Act
		await _handler.HandleAsync(chatId, args, cancellationToken);

		// Assert
       _botClientMock.Verify(client => client.SendRequest(
            It.Is<SendMessageRequest>(x => x.ChatId == chatId && x.Text == "You're already registered to receive Hiragana practice messages."), 
            It.IsAny<CancellationToken>()), Times.Once);

		_userRepositoryMock.Verify(repo => repo.GetByTelegramIdAsync(chatId, cancellationToken), Times.Once);
		_userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Domain.User>(), cancellationToken), Times.Never);
		_userRepositoryMock.Verify(repo => repo.SaveChangesAsync(cancellationToken), Times.Never);
	}

	[Fact]
	public async Task HandleAsync_ShouldRegisterNewUserAndSendWelcomeMessage_WhenUserNotFound()
	{
		// Arrange
		long chatId = 123456789L;
		string[] args = Array.Empty<string>();
		CancellationToken cancellationToken = CancellationToken.None;

		_userRepositoryMock
			.Setup(repo => repo.GetByTelegramIdAsync(chatId, cancellationToken))
			.ReturnsAsync((Domain.User?)null);

		ChatFullInfo chatInfo = new ChatFullInfo
		{
			Username = "testuser"
		};

        _botClientMock
			.Setup(client => client.SendRequest(
				It.IsAny<GetChatRequest>(), 
				It.IsAny<CancellationToken>()))
            .ReturnsAsync(chatInfo);


		// Act
		await _handler.HandleAsync(chatId, args, cancellationToken);

		// Assert
		_userRepositoryMock.Verify(repo => repo.AddAsync(
			It.Is<Domain.User>(u => u.TelegramId == chatId && u.Username == "testuser"),
			cancellationToken), Times.Once);

		_userRepositoryMock.Verify(repo => repo.SaveChangesAsync(cancellationToken), Times.Once);

			       _botClientMock.Verify(client => client.SendRequest(
            It.Is<SendMessageRequest>(x => x.ChatId == chatId && x.Text ==	"Welcome to NihongoBot! You're now registered to receive Hiragana practice messages."), 
            It.IsAny<CancellationToken>()), Times.Once);
	}
}
