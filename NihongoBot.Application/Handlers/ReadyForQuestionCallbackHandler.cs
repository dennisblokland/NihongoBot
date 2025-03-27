

using NihongoBot.Application.Models;
using NihongoBot.Application.Services;
using NihongoBot.Domain.Entities;
using NihongoBot.Domain.Interfaces.Repositories;

using Telegram.Bot;

namespace NihongoBot.Application.Handlers;

public class ReadyForQuestionCallbackHandler : ITelegramCallbackHandler<ReadyCallbackData>
{
	private readonly ITelegramBotClient _botClient;
	private readonly HiraganaService _hiraganaService;
	private readonly IQuestionRepository _questionRepository;


	public ReadyForQuestionCallbackHandler(HiraganaService hiraganaService, ITelegramBotClient botClient, IQuestionRepository questionRepository)
	{
		_hiraganaService = hiraganaService;
		_botClient = botClient;
		_questionRepository = questionRepository;
	}


	public async Task HandleAsync(long chatId, ReadyCallbackData callbackData, CancellationToken cancellationToken)
	{
		Question? question = await _questionRepository.FindByIdAsync(callbackData.QuestionId, cancellationToken);
		if (question == null)
		{
			await _botClient.SendMessage(chatId, "Question not found.", cancellationToken: cancellationToken);
			return;
		}

		if (question.IsAccepted || question.IsAnswered || question.IsExpired) 
		{
			return;
		}

		await _hiraganaService.SendQuestion(chatId, question, cancellationToken);
	}
}
