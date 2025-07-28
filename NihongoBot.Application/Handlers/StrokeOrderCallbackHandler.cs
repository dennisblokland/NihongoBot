using NihongoBot.Application.Models;
using NihongoBot.Application.Services;

using Telegram.Bot;

namespace NihongoBot.Application.Handlers;

public class StrokeOrderCallbackHandler : ITelegramCallbackHandler<StrokeOrderCallbackData>
{
	private readonly ITelegramBotClient _botClient;
	private readonly HiraganaService _hiraganaService;

	public StrokeOrderCallbackHandler(
		ITelegramBotClient botClient,
		HiraganaService hiraganaService)
	{
		_botClient = botClient;
		_hiraganaService = hiraganaService;
	}

	public async Task HandleAsync(long chatId, StrokeOrderCallbackData callbackData, CancellationToken cancellationToken)
	{
		// Acknowledge the callback
		await _botClient.AnswerCallbackQuery(chatId.ToString(), cancellationToken: cancellationToken);

		// Send the stroke order animation
		await _hiraganaService.SendStrokeOrderAnimation(chatId, callbackData.Character, cancellationToken);
	}
}