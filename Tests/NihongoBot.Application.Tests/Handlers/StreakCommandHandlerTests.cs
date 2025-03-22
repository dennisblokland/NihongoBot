using AutoFixture;

using Moq;

using NihongoBot.Application.Handlers;
using NihongoBot.Application.Tests.Extentions;
using NihongoBot.Domain.Interfaces.Repositories;

using Telegram.Bot;
using Telegram.Bot.Requests;

namespace NihongoBot.Application.Tests.Handlers;

public class StreakCommandHandlerTests
{
	private readonly Mock<ITelegramBotClient> _botClientMock = new();
	private readonly Mock<IUserRepository> _userRepositoryMock = new();
	private readonly StreakCommandHandler _handler;
	private readonly Fixture _fixture = new();

	public StreakCommandHandlerTests()
	{
		_handler = new StreakCommandHandler(_botClientMock.Object, _userRepositoryMock.Object);
	}

	[Fact]
	public async Task HandleAsync_ShouldSendNotRegisteredMessage_WhenUserNotFound()
	{
		// Arrange
		long chatId = 123456789L;
		string[] args = Array.Empty<string>();
		CancellationToken cancellationToken = CancellationToken.None;

		_userRepositoryMock
			.Setup(repo => repo.GetByTelegramIdAsync(chatId, cancellationToken))
			.ReturnsAsync((Domain.User?)null);

		// Act
		await _handler.HandleAsync(chatId, args, cancellationToken);

		// Assert
       _botClientMock.Verify(client => client.SendRequest(
            It.Is<SendMessageRequest>(x => x.ChatId == chatId && x.Text == "You are not registered."), 
            It.IsAny<CancellationToken>()), Times.Once);

		_userRepositoryMock.Verify(repo => repo.GetByTelegramIdAsync(chatId, cancellationToken), Times.Once);
	}

	[Fact]
	public async Task HandleAsync_ShouldSendStreakMessage_WhenUserFound()
	{
		// Arrange
		long chatId = 123456789L;
		string[] args = Array.Empty<string>();
		CancellationToken cancellationToken = CancellationToken.None;

		Domain.User user = _fixture.Build<Domain.User>()
			.WithPrivate(u => u.TelegramId, chatId)
			.WithPrivate(u => u.Streak, 5)
			.Create();

		_userRepositoryMock
			.Setup(repo => repo.GetByTelegramIdAsync(chatId, cancellationToken))
			.ReturnsAsync(user);

		// Act
		await _handler.HandleAsync(chatId, args, cancellationToken);

		// Assert
		_botClientMock.Verify(client => client.SendRequest(
            It.Is<SendMessageRequest>(x => x.ChatId == chatId && x.Text =="Your current streak is 5."), 
            It.IsAny<CancellationToken>()), Times.Once);

		_userRepositoryMock.Verify(repo => repo.GetByTelegramIdAsync(chatId, cancellationToken), Times.Once);
	}
}
