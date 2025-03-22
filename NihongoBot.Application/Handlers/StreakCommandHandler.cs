using NihongoBot.Domain.Interfaces.Repositories;

using Telegram.Bot;

namespace NihongoBot.Application.Handlers;

public class StreakCommandHandler : ITelegramCommandHandler
{
	private readonly ITelegramBotClient _botClient;
	private readonly IUserRepository _userRepository;

	public StreakCommandHandler(ITelegramBotClient botClient, IUserRepository userRepository)
	{
		_botClient = botClient;
		_userRepository = userRepository;
	}

	public async Task HandleAsync(long chatId, string[] args, CancellationToken cancellationToken)
	{
		Domain.User? user = await _userRepository.GetByTelegramIdAsync(chatId, cancellationToken);
		if (user == null)
		{
			await _botClient.SendMessage(chatId, "You are not registered.", cancellationToken: cancellationToken);
			return;
		}

		int streak = user.Streak;
		await _botClient.SendMessage(chatId, $"Your current streak is {streak}.", cancellationToken: cancellationToken);
	}
}
