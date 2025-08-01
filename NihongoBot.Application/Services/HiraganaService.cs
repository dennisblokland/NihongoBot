using Microsoft.Extensions.Logging;

using NihongoBot.Application.Enums;
using NihongoBot.Application.Interfaces;
using NihongoBot.Domain.Aggregates.Kana;
using NihongoBot.Domain.Entities;
using NihongoBot.Domain.Enums;
using NihongoBot.Domain.Interfaces.Repositories;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

using Newtonsoft.Json;

namespace NihongoBot.Application.Services;

public class HiraganaService
{
	private readonly ITelegramBotClient _botClient;
	private readonly IQuestionRepository _questionRepository;
	private readonly IKanaRepository _kanaRepository;
	private readonly IImageCacheService _imageCacheService;
	private readonly IStrokeOrderService _strokeOrderService;
	private readonly ILogger<HiraganaService> _logger;

	public HiraganaService(
		IQuestionRepository questionRepository,
		IKanaRepository kanaRepository,
		ITelegramBotClient botClient,
		IImageCacheService imageCacheService,
		IStrokeOrderService strokeOrderService,
		ILogger<HiraganaService> logger
	)
	{
		_botClient = botClient;
		_logger = logger;
		_questionRepository = questionRepository;
		_kanaRepository = kanaRepository;
		_imageCacheService = imageCacheService;
		_strokeOrderService = strokeOrderService;
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

		Question question = new()
		{
			UserId = userId,
			QuestionType = QuestionType.Hiragana,
			QuestionText = kana.Character,
			CorrectAnswer = kana.Romaji,
			TimeLimit = 1,
		};

		question = await _questionRepository.AddAsync(question, cancellationToken);
		await _questionRepository.SaveChangesAsync(cancellationToken);

		await SendReadyMessageAsync(telegramId, question.Id, cancellationToken);
	}

	public async Task SendMultipleChoiceHiraganaMessage(long telegramId, Guid userId, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Sending Multiple Choice Hiragana message at {Time}", DateTime.Now);

		Kana? kana = await _kanaRepository.GetRandomAsync(KanaType.Hiragana, cancellationToken);

		if (kana == null)
		{
			_logger.LogWarning("No Kana found in the database.");
			return;
		}

		// Get wrong answers
		List<Kana> wrongAnswers = await _kanaRepository.GetWrongAnswersAsync(kana.Romaji, KanaType.Hiragana, 3, cancellationToken);
		
		// Create list of all options (correct + wrong)
		List<string> allOptions = [kana.Romaji, .. wrongAnswers.Select(w => w.Romaji)];
		
		// Shuffle the options
		Random random = new Random();
		allOptions = allOptions.OrderBy(x => random.Next()).ToList();

		Question question = new()
		{
			UserId = userId,
			QuestionType = QuestionType.MultipleChoiceHiragana,
			QuestionText = kana.Character,
			CorrectAnswer = kana.Romaji,
			MultipleChoiceOptions = JsonConvert.SerializeObject(allOptions),
			TimeLimit = 1,
		};

		question = await _questionRepository.AddAsync(question, cancellationToken);
		await _questionRepository.SaveChangesAsync(cancellationToken);

		await SendMultipleChoiceQuestion(telegramId, question, cancellationToken);
	}

	public async Task SendQuestion(long telegramId, Question question, CancellationToken cancellationToken)
	{
		byte[] imageBytes = await _imageCacheService.GetOrGenerateImageAsync(question.QuestionText);
		Stream stream = new MemoryStream(imageBytes);

		Message message = await _botClient.SendPhoto(telegramId,
			InputFile.FromStream(stream, "hiragana.png"),
			caption: $"What is the Romaji for this {question.QuestionText} character?",
			cancellationToken: cancellationToken);

		question.MessageId = message.MessageId;
		question.SentAt = DateTime.UtcNow;
		question.IsAccepted = true;
		_questionRepository.Update(question);

		await _questionRepository.SaveChangesAsync(cancellationToken);
	}

	public async Task SendMultipleChoiceQuestion(long telegramId, Question question, CancellationToken cancellationToken)
	{
		byte[] imageBytes = await _imageCacheService.GetOrGenerateImageAsync(question.QuestionText);
		Stream stream = new MemoryStream(imageBytes);

		// Parse the multiple choice options
		List<string> options = JsonConvert.DeserializeObject<List<string>>(question.MultipleChoiceOptions ?? "[]") ?? new List<string>();
		
		// Create inline keyboard with options
		List<List<InlineKeyboardButton>> keyboardButtons = new List<List<InlineKeyboardButton>>();
		
		foreach (string option in options)
		{
			string callbackData = $"{(int)CallBackType.MultipleChoiceAnswer}|{question.Id}|{option}";
			keyboardButtons.Add(new List<InlineKeyboardButton>
			{
				InlineKeyboardButton.WithCallbackData(option, callbackData)
			});
		}

		InlineKeyboardMarkup inlineKeyboard = new(keyboardButtons);

		Message message = await _botClient.SendPhoto(telegramId,
			InputFile.FromStream(stream, "hiragana.png"),
			caption: $"What is the Romaji for this {question.QuestionText} character?",
			replyMarkup: inlineKeyboard,
			cancellationToken: cancellationToken);

		question.MessageId = message.MessageId;
		question.SentAt = DateTime.UtcNow;
		question.IsAccepted = true;
		_questionRepository.Update(question);

		await _questionRepository.SaveChangesAsync(cancellationToken);
	}

	public async Task SendStrokeOrderAnimation(long telegramId, string character, CancellationToken cancellationToken)
	{
		if (!_strokeOrderService.HasStrokeOrderAnimation(character))
		{
			await _botClient.SendMessage(telegramId, 
				$"Sorry, stroke order animation is not available for the character {character}.",
				cancellationToken: cancellationToken);
			return;
		}

		try
		{
			byte[]? animationBytes = await _strokeOrderService.GetStrokeOrderAnimationBytesAsync(character, cancellationToken);
			
			if (animationBytes == null)
			{
				await _botClient.SendMessage(telegramId, 
					$"Sorry, stroke order animation could not be loaded for the character {character}.",
					cancellationToken: cancellationToken);
				return;
			}

			Stream animationStream = new MemoryStream(animationBytes);
			
			await _botClient.SendAnimation(telegramId,
				InputFile.FromStream(animationStream, $"stroke_order_{character}.gif"),
				caption: $"Stroke order for {character}",
				cancellationToken: cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send stroke order animation for character: {Character}", character);
			await _botClient.SendMessage(telegramId, 
				$"Sorry, there was an error loading the stroke order animation for {character}.",
				cancellationToken: cancellationToken);
		}
	}

	private async Task SendReadyMessageAsync(long telegramId, Guid QuestionId, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Sending 'Ready for another challenge?' message at {Time}", DateTime.Now);

		// Create compact callback data format: "{type}|{guid}" where type is ReadyForQuestion
		string callbackData = $"{(int)CallBackType.ReadyForQuestion}|{QuestionId}";
		
		InlineKeyboardMarkup inlineKeyboard = new(
		new[]
		{
			InlineKeyboardButton.WithCallbackData("Ready", callbackData)
		});

		await _botClient.SendMessage(
			chatId: telegramId,
			text: "Are you ready for another challenge?",
			replyMarkup: inlineKeyboard,
			cancellationToken: cancellationToken);
	}
}
