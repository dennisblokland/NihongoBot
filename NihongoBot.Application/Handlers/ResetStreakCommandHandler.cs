using NihongoBot.Domain.Interfaces.Repositories;

using Telegram.Bot;

namespace NihongoBot.Application.Handlers;

public class ResetStreakCommandHandler : ITelegramCommandHandler
{
	private readonly ITelegramBotClient _botClient;
	private readonly IUserRepository _userRepository;

	public ResetStreakCommandHandler(ITelegramBotClient botClient, IUserRepository userRepository)
	{
		_botClient = botClient;
		_userRepository = userRepository;
	}

	public async Task HandleAsync(long chatId, string[] args, CancellationToken cancellationToken)
	{
		Domain.User? user = await _userRepository.GetByTelegramIdAsync(chatId, cancellationToken);
		if (user == null)
		{
			return;
		}

		user.ResetStreak();
		await _userRepository.SaveChangesAsync(cancellationToken);
		await _botClient.SendMessage(chatId, "Your streak has been reset.", cancellationToken: cancellationToken);
	}
}
