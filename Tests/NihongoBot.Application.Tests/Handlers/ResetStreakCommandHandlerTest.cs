using AutoFixture;

using Moq;
using NihongoBot.Application.Handlers;
using NihongoBot.Domain.Interfaces.Repositories;
using NihongoBot.Application.Tests.Extentions;

using Telegram.Bot;
using Telegram.Bot.Requests;

namespace NihongoBot.Application.Tests.Handlers;

public class ResetStreakCommandHandlerTest
{
	private readonly Mock<ITelegramBotClient> _botClientMock = new();
	private readonly Mock<IUserRepository> _userRepositoryMock = new();
	private readonly ResetStreakCommandHandler _handler;
	private readonly Fixture _fixture = new();

	public ResetStreakCommandHandlerTest()
	{
		_handler = new ResetStreakCommandHandler(_botClientMock.Object, _userRepositoryMock.Object);
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
		_botClientMock.Verify(client => client.SendRequest(
			It.IsAny<SendMessageRequest>(),
			It.IsAny<CancellationToken>()), Times.Never);

		_userRepositoryMock.Verify(repo => repo.SaveChangesAsync(cancellationToken), Times.Never);
	}

	[Fact]
	public async Task HandleAsync_ShouldResetStreakAndSendMessage_WhenUserFound()
	{
		// Arrange
		long chatId = 123456789L;
		string[] args = Array.Empty<string>();
		CancellationToken cancellationToken = CancellationToken.None;

		Domain.User user = _fixture.Build<Domain.User>()
			.WithPrivate(u => u.TelegramId, chatId)
			.WithPrivate(u => u.Streak, 10)
			.Create();

		_userRepositoryMock
			.Setup(repo => repo.GetByTelegramIdAsync(chatId, cancellationToken))
			.ReturnsAsync(user);

		// Act
		await _handler.HandleAsync(chatId, args, cancellationToken);

		// Assert
		Assert.Equal(0, user.Streak); // Verify that the streak was reset
		_userRepositoryMock.Verify(repo => repo.SaveChangesAsync(cancellationToken), Times.Once);
		_botClientMock.Verify(client => client.SendRequest(
	 It.Is<SendMessageRequest>(x => x.ChatId == chatId && x.Text == "Your streak has been reset."),
	 It.IsAny<CancellationToken>()), Times.Once);
	}
}
