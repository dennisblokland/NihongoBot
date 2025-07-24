using NihongoBot.Application.Services;
using NihongoBot.Domain.Interfaces.Repositories;

using Telegram.Bot;

namespace NihongoBot.Application.Handlers;

public class MultipleChoiceCommandHandler : ITelegramCommandHandler
{
	private readonly ITelegramBotClient _botClient;
	private readonly HiraganaService _hiraganaService;
	private readonly IUserRepository _userRepository;

	public MultipleChoiceCommandHandler(ITelegramBotClient botClient, HiraganaService hiraganaService, IUserRepository userRepository)
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

		await _hiraganaService.SendMultipleChoiceHiraganaMessage(chatId, user.Id, cancellationToken);
	}
}