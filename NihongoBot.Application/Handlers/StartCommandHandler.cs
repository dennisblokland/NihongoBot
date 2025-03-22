using NihongoBot.Domain.Interfaces.Repositories;

using Telegram.Bot;

namespace NihongoBot.Application.Handlers;

public class StartCommandHandler : ITelegramCommandHandler
{
	private readonly ITelegramBotClient _botClient;
	private readonly IUserRepository _userRepository;

	public StartCommandHandler(ITelegramBotClient botClient, IUserRepository userRepository)
	{
		_botClient = botClient;
		_userRepository = userRepository;
	}

	public async Task HandleAsync(long chatId, string[] args, CancellationToken cancellationToken)
	{
		Domain.User? user = await _userRepository.GetByTelegramIdAsync(chatId, cancellationToken);
		if (user != null)
		{
			await _botClient.SendMessage(chatId, "You're already registered to receive Hiragana practice messages.", cancellationToken: cancellationToken);
			return;
		}

		Telegram.Bot.Types.ChatFullInfo chat = await _botClient.GetChat(chatId, cancellationToken);
		await _userRepository.AddAsync(new Domain.User(chatId, chat.Username), cancellationToken);
		await _userRepository.SaveChangesAsync(cancellationToken);
		await _botClient.SendMessage(chatId, "Welcome to NihongoBot! You're now registered to receive Hiragana practice messages.", cancellationToken: cancellationToken);
	}
}
