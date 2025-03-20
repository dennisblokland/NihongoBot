using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using NihongoBot.Application.Helpers;
using NihongoBot.Domain.Aggregates.Hiragana;
using NihongoBot.Domain.Entities;
using NihongoBot.Domain.Enums;
using NihongoBot.Persistence;

using Telegram.Bot;
using Telegram.Bot.Types;


namespace NihongoBot.Application.Services;

public class HiraganaService
{
	private readonly ITelegramBotClient _botClient;
	private readonly ILogger<HiraganaService> _logger;
	private readonly AppDbContext _dbContext;

	public HiraganaService(ITelegramBotClient botClient, ILogger<HiraganaService> logger, AppDbContext dbContext)
	{
		_botClient = botClient;
		_logger = logger;
		_dbContext = dbContext;
	}

	public async Task SendHiraganaMessage(long telegramId, Guid userId, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Sending Hiragana message at {Time}", DateTime.Now);

		Kana? kana = await _dbContext.Kanas.OrderBy(h => Guid.NewGuid()).FirstOrDefaultAsync(cancellationToken);
		if (kana == null)
		{
			_logger.LogWarning("No Kana found in the database.");
			return;
		}

		Dictionary<Kana, byte[]> renderedKana = [];

		byte[] imageBytes = KanaRenderer.RenderCharacterToImage(kana.Character);
		renderedKana.Add(kana, imageBytes);

		//take random hiragana character from the list
		KeyValuePair<Kana, byte[]> hiragana = renderedKana.OrderBy(h => Guid.NewGuid()).First();
		Stream stream = new MemoryStream(hiragana.Value);
		Message message = await _botClient.SendPhoto(telegramId,
		InputFile.FromStream(stream, "hiragana.png"),
		caption: $"What is the Romaji for this {hiragana.Key.Character} Hiragana character?");

		//save the Question to the database
		Question question = new()
		{
			UserId = userId,
			QuestionType = QuestionType.Hiragana,
			QuestionText = hiragana.Key.Character,
			CorrectAnswer = hiragana.Key.Romaji,
			SentAt = DateTime.UtcNow,
			MessageId = message.MessageId,
			TimeLimit = 5 // Set the time limit to 5 minutes
		};

		_dbContext.Questions.Add(question);
		await _dbContext.SaveChangesAsync(cancellationToken);
	}
}
