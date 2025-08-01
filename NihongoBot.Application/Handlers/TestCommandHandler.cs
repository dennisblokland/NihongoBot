
using NihongoBot.Application.Services;
using NihongoBot.Domain.Interfaces.Repositories;

using Telegram.Bot;

namespace NihongoBot.Application.Handlers;

public class TestCommandHandler : ITelegramCommandHandler
{
	private readonly ITelegramBotClient _botClient;
	private readonly HiraganaService _hiraganaService;
	private readonly IUserRepository _userRepository;

	public TestCommandHandler(ITelegramBotClient botClient, HiraganaService hiraganaService, IUserRepository userRepository)
	{
		_botClient = botClient;
		_hiraganaService = hiraganaService;
		_userRepository = userRepository;
	}

	public async Task HandleAsync(long chatId, string[] args, CancellationToken cancellationToken)
	{
		Domain.User? user = await _userRepository.GetByTelegramIdAsync(chatId, cancellationToken);
		if (user == null)
		{
			await _botClient.SendMessage(chatId, "You need to register first using /start command.", cancellationToken: cancellationToken);
			return;
		}

		// Check if user wants multiple choice
		if (args.Length > 0 && args[0].Equals("mc", StringComparison.OrdinalIgnoreCase))
		{
			await _hiraganaService.SendMultipleChoiceHiraganaMessage(chatId, user.Id, cancellationToken);
		}
		else
		{
			await _hiraganaService.SendHiraganaMessage(chatId, user.Id, cancellationToken);
		}
	}
}
