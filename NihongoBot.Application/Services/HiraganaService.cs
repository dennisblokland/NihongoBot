using System.Text.Json;

using Microsoft.Extensions.Logging;

using NihongoBot.Application.Helpers;
using NihongoBot.Application.Models;
using NihongoBot.Domain.Aggregates.Kana;
using NihongoBot.Domain.Entities;
using NihongoBot.Domain.Enums;
using NihongoBot.Domain.Interfaces.Repositories;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace NihongoBot.Application.Services;

public class HiraganaService
{
	private readonly ITelegramBotClient _botClient;
	private readonly IQuestionRepository _questionRepository;
	private readonly IKanaRepository _kanaRepository;
	private readonly ILogger<HiraganaService> _logger;

	public HiraganaService(
		IQuestionRepository questionRepository,
		IKanaRepository kanaRepository,
		ITelegramBotClient botClient,
		ILogger<HiraganaService> logger
	)
	{
		_botClient = botClient;
		_logger = logger;
		_questionRepository = questionRepository;
		_kanaRepository = kanaRepository;
	}

	public async Task SendHiraganaMessage(long telegramId, Guid userId, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Sending Hiragana message at {Time}", DateTime.Now);

		Kana? kana = await _kanaRepository.GetRandomAsync(KanaType.Hiragana, cancellationToken);

		if (kana == null)
		{
			_logger.LogWarning("No Kana found in the database.");
			return;
		}
		//save the Question to the database
		Question question = new()
		{
			UserId = userId,
			QuestionType = QuestionType.Hiragana,
			QuestionText = kana.Character,
			CorrectAnswer = kana.Romaji,
			SentAt = DateTime.UtcNow,
			TimeLimit = 1,
		};

		question = await _questionRepository.AddAsync(question, cancellationToken);
		await _questionRepository.SaveChangesAsync(cancellationToken);

		await SendReadyMessageAsync(telegramId, question.Id, cancellationToken);
	}

	public async Task SendQuestion(long telegramId, Question question, CancellationToken cancellationToken)
	{
		byte[] imageBytes = KanaRenderer.RenderCharacterToImage(question.QuestionText);
		Stream stream = new MemoryStream(imageBytes);

		Message message = await _botClient.SendPhoto(telegramId,
		InputFile.FromStream(stream, "hiragana.png"),
		caption: $"What is the Romaji for this {question.QuestionText} character?", cancellationToken: cancellationToken);

		question.MessageId = message.MessageId;
		question.IsAccepted = true;
		await _questionRepository.SaveChangesAsync(cancellationToken);
	}

	private async Task SendReadyMessageAsync(long telegramId, Guid QuestionId, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Sending 'Ready for another challenge?' message at {Time}", DateTime.Now);

		ReadyCallbackData callbackData = new()
		{
			QuestionId = QuestionId
		};
		InlineKeyboardMarkup inlineKeyboard = new(
		new[]
		{
			InlineKeyboardButton.WithCallbackData("Ready",  JsonSerializer.Serialize(callbackData))
		});

		await _botClient.SendMessage(
			chatId: telegramId,
			text: "Are you ready for another challenge?",
			replyMarkup: inlineKeyboard,
			cancellationToken: cancellationToken);
	}

	public async Task HandleReadyButtonClick(long telegramId, Guid userId, CancellationToken cancellationToken)
	{
		await SendHiraganaMessage(telegramId, userId, cancellationToken);
	}
}
