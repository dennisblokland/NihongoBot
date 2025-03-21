using Microsoft.Extensions.Logging;

using NihongoBot.Application.Helpers;
using NihongoBot.Domain.Aggregates.Kana;
using NihongoBot.Domain.Entities;
using NihongoBot.Domain.Enums;
using NihongoBot.Domain.Interfaces.Repositories;

using Telegram.Bot;
using Telegram.Bot.Types;


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


		byte[] imageBytes = KanaRenderer.RenderCharacterToImage(kana.Character);
		Stream stream = new MemoryStream(imageBytes);

		Message message = await _botClient.SendPhoto(telegramId,
		InputFile.FromStream(stream, "hiragana.png"),
		caption: $"What is the Romaji for this {kana.Character} Hiragana character?", cancellationToken: cancellationToken);

		//save the Question to the database
		Question question = new()
		{
			UserId = userId,
			QuestionType = QuestionType.Hiragana,
			QuestionText = kana.Character,
			CorrectAnswer = kana.Romaji,
			SentAt = DateTime.UtcNow,
			MessageId = message.MessageId,
			TimeLimit = 5 // Set the time limit to 5 minutes
		};

		await _questionRepository.AddAsync(question, cancellationToken);
		await _questionRepository.SaveChangesAsync(cancellationToken);
	}
}
