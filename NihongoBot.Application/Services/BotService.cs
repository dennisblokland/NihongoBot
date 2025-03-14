using Microsoft.Extensions.Logging;

using NihongoBot.Domain.Aggregates.Hiragana;
using NihongoBot.Domain.Entities;
using NihongoBot.Persistence;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NihongoBot.Application.Services;

public class BotService
{
	private readonly ITelegramBotClient _botClient;
	private readonly AppDbContext _dbContext;
	private readonly HiraganaService _hiraganaService;
	private readonly ILogger<BotService> _logger;

	public BotService(
		ITelegramBotClient botClient,
		AppDbContext dbContext,
		HiraganaService hiraganaService,
		ILogger<BotService> logger)
	{
		_botClient = botClient;
		_dbContext = dbContext;
		_hiraganaService = hiraganaService;
		_logger = logger;
	}

	public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
	{
		if (update.Type == UpdateType.Message && update.Message?.Text != null)
		{
			long chatId = update.Message.Chat.Id;
			string userMessage = update.Message.Text.Trim().ToLower();
			if (userMessage.StartsWith("/"))
			{
				await HandleCommand(chatId, userMessage, cancellationToken);
			}
			else // For now, if it's not a command, then it might be a answer to a question I've asked. (Can be made into a session command handler.)
			{
				await ProcessAnswer(chatId, userMessage, cancellationToken);
			}
		}
	}

	private async Task HandleCommand(long chatId, string command, CancellationToken cancellationToken)
	{
		ChatFullInfo chat = await _botClient.GetChat(chatId);
		Domain.User? user = _dbContext.Users.FirstOrDefault(u => u.TelegramId == chatId);

		switch (command)
		{
			case "/start":
				await RegisterUser(chatId, cancellationToken);
				break;
			case "/stop":
				if (user == null)
				{
					return;
				}
				_dbContext.Users.Remove(user);
				await _dbContext.SaveChangesAsync(cancellationToken);
				await _botClient.SendMessage(chatId, "You've been unregistered from receiving Hiragana practice messages.", cancellationToken: cancellationToken);
				break;
			case "/streak":
				int streak = _dbContext.Users.FirstOrDefault(u => u.TelegramId == chatId)?.Streak ?? 0;
				await _botClient.SendMessage(chatId, $"Your current streak is {streak}.", cancellationToken: cancellationToken);
				break;
			case "/resetstreak":
				if (user == null)
				{
					return;
				}
				user.ResetStreak();
				await _dbContext.SaveChangesAsync(cancellationToken);
				await _botClient.SendMessage(chatId, "Your streak has been reset.", cancellationToken: cancellationToken);
				break;
			case "/test":
				if (user == null)
				{
					return;
				}
				await _hiraganaService.SendHiraganaMessage(chatId, user.Id);
				break;
			default:
				await _botClient.SendMessage(chatId, "Command not recognized.", cancellationToken: cancellationToken);
				break;
		}
	}

	private async Task RegisterUser(long chatId, CancellationToken cancellationToken)
	{
		// Check if user is already registered
		if (_dbContext.Users.Any(u => u.TelegramId == chatId))
		{
			await _botClient.SendMessage(chatId, "You're already registered to receive Hiragana practice messages.");
			return;
		}

		ChatFullInfo chat = await _botClient.GetChat(chatId);

		_dbContext.Users.Add(new Domain.User(chatId, chat.Username));
		await _dbContext.SaveChangesAsync(cancellationToken);
		await _botClient.SendMessage(chatId, "Welcome to NihongoBot! You're now registered to receive Hiragana practice messages.");

	}

	private async Task ProcessAnswer(long chatId, string userMessage, CancellationToken cancellationToken)
	{
		Domain.User? user = _dbContext.Users.FirstOrDefault(u => u.TelegramId == chatId);

		if (user == null)
		{
			await RegisterUser(chatId, cancellationToken);
			user = _dbContext.Users.FirstOrDefault(u => u.TelegramId == chatId);

			return;
		}

		Question? question = _dbContext.Questions
		.OrderBy(q => q.SentAt)
		.FirstOrDefault(q =>
			q.UserId == user.Id &&
			q.IsAnswered == false &&
			q.IsExpired == false
		);

		if (question == null)
			return;

		if (userMessage == question.CorrectAnswer)
		{
			question.IsAnswered = true;
			user.IncreaseStreak();
			await _dbContext.SaveChangesAsync(cancellationToken);

			Kana? kana = _dbContext.Kanas.FirstOrDefault(k => k.Character == question.QuestionText);
			if (kana == null)
			{
				return;
			}

			string message = $"Correct! The Romaji for {question.QuestionText} is {question.CorrectAnswer}.\nYour current streak is **{user.Streak}**.";
			if (kana.Variants != null && kana.Variants.Count > 0)
			{
				message += "Variants: \n";
				foreach (var variant in kana.Variants)
				{
					message += "   " + variant.Character + " is " + variant.Romaji + "\n";
				}
			}
			await _botClient.SendMessage(chatId,
				message,
				ParseMode.Markdown, cancellationToken: cancellationToken);
		}
		else
		{
			question.Attempts++;

			if (question.Attempts >= 3)
			{
				question.IsExpired = true;
				user.ResetStreak();
				await _dbContext.SaveChangesAsync(cancellationToken);

				await _botClient.SendMessage(chatId, "You've reached the maximum number of attempts. The correct answer was " + question.CorrectAnswer, cancellationToken: cancellationToken);
				return;
			}
			await _dbContext.SaveChangesAsync(cancellationToken);

			await _botClient.SendMessage(chatId, "Incorrect. Please try again.", cancellationToken: cancellationToken);
		}
	}
	public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
	{
		var errorMessage = exception switch
		{
			ApiRequestException apiRequestException => $"Telegram API Error: [{apiRequestException.ErrorCode}] {apiRequestException.Message}",
			_ => exception.ToString()
		};

		_logger.LogError(errorMessage);
		return Task.CompletedTask;
	}
}
