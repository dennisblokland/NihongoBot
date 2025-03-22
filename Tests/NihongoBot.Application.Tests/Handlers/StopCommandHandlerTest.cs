using AutoFixture;

using Moq;

using NihongoBot.Application.Handlers;
using NihongoBot.Application.Tests.Extentions;
using NihongoBot.Domain.Interfaces.Repositories;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Requests;

namespace NihongoBot.Application.Tests.Handlers;

public class StopCommandHandlerTest
{
	private readonly Mock<ITelegramBotClient> _botClientMock = new();
	private readonly Mock<IUserRepository> _userRepositoryMock = new();
	private readonly StopCommandHandler _handler;
	private readonly Fixture _fixture = new();

	public StopCommandHandlerTest()
	{
		_handler = new StopCommandHandler(_botClientMock.Object, _userRepositoryMock.Object);
	}
	[Fact]
	public async Task HandleAsync_ShouldDoNothing_WhenUserNotFound()
	{
		// Arrange
		long chatId = 123456789L;
		string[] args = Array.Empty<string>();
		CancellationToken cancellationToken = CancellationToken.None;

		_userRepositoryMock
			.Setup(repo => repo.GetByTelegramIdAsync(chatId, cancellationToken))
			.ReturnsAsync((Domain.User?) null);

		// Act
		await _handler.HandleAsync(chatId, args, cancellationToken);

		// Assert
		_userRepositoryMock.Verify(repo => repo.Remove(It.IsAny<Domain.User>()), Times.Never);
		_userRepositoryMock.Verify(repo => repo.SaveChangesAsync(cancellationToken), Times.Never);
		_botClientMock.Verify(client => client.SendRequest(
			It.IsAny<SendMessageRequest>(),
			It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task HandleAsync_ShouldRemoveUserAndSendMessage_WhenUserFound()
	{
		// Arrange
		long chatId = 123456789L;
		string[] args = Array.Empty<string>();
		CancellationToken cancellationToken = CancellationToken.None;

		Domain.User user = _fixture.Build<Domain.User>()
			.WithPrivate(u => u.TelegramId, chatId)
			.Create();

		_userRepositoryMock
			.Setup(repo => repo.GetByTelegramIdAsync(chatId, cancellationToken))
			.ReturnsAsync(user);

		// Act
		await _handler.HandleAsync(chatId, args, cancellationToken);

		// Assert
		_userRepositoryMock.Verify(repo => repo.Remove(user), Times.Once);
		_userRepositoryMock.Verify(repo => repo.SaveChangesAsync(cancellationToken), Times.Once);
		_botClientMock.Verify(client => client.SendRequest(
			It.Is<SendMessageRequest>(x => x.ChatId == chatId && x.Text == "You've been unregistered from receiving Hiragana practice messages."),
			It.IsAny<CancellationToken>()), Times.Once);
	}
}
