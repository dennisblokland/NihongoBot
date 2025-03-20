using Microsoft.Extensions.Logging;

using NihongoBot.Domain.Aggregates.Kana;
using NihongoBot.Domain.Entities;
using NihongoBot.Domain.Interfaces.Repositories;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NihongoBot.Application.Services;

public class BotService
{
	private readonly IUserRepository _userRepository;
	private readonly IQuestionRepository _questionRepository;
	private readonly IKanaRepository _kanaRepository;
	private readonly ITelegramBotClient _botClient;
	private readonly HiraganaService _hiraganaService;
	private readonly ILogger<BotService> _logger;

	public BotService(
		IUserRepository userRepository,
		IQuestionRepository questionRepository,
		IKanaRepository kanaRepository,
		ITelegramBotClient botClient,
		HiraganaService hiraganaService,
		ILogger<BotService> logger)
	{
		_userRepository = userRepository;
		_botClient = botClient;
		_questionRepository = questionRepository;
		_kanaRepository = kanaRepository;
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
		Domain.User? user = await _userRepository.GetByTelegramIdAsync(chatId);

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
				_userRepository.Remove(user);
				await _userRepository.SaveChangesAsync(cancellationToken);
				await _botClient.SendMessage(chatId, "You've been unregistered from receiving Hiragana practice messages.", cancellationToken: cancellationToken);
				break;
			case "/streak":
				int streak = user?.Streak ?? 0;
				await _botClient.SendMessage(chatId, $"Your current streak is {streak}.", cancellationToken: cancellationToken);
				break;
			case "/resetstreak":
				if (user == null)
				{
					return;
				}
				user.ResetStreak();
				await _userRepository.SaveChangesAsync(cancellationToken);
				await _botClient.SendMessage(chatId, "Your streak has been reset.", cancellationToken: cancellationToken);
				break;
			case "/test":
				if (user == null)
				{
					return;
				}
				await _hiraganaService.SendHiraganaMessage(chatId, user.Id, cancellationToken);
				break;
			default:
				await _botClient.SendMessage(chatId, "Command not recognized.", cancellationToken: cancellationToken);
				break;
		}
	}

	private async Task<Domain.User> RegisterUser(long chatId, CancellationToken cancellationToken)
	{
		// Check if user is already registered
		Domain.User? user = await _userRepository.GetByTelegramIdAsync(chatId);
		if (user != null)
		{
			await _botClient.SendMessage(chatId, "You're already registered to receive Hiragana practice messages.");
			return user;
		}

		ChatFullInfo chat = await _botClient.GetChat(chatId);

		user = await _userRepository.AddAsync(new Domain.User(chatId, chat.Username), cancellationToken);
		await _userRepository.SaveChangesAsync(cancellationToken);
		await _botClient.SendMessage(chatId, "Welcome to NihongoBot! You're now registered to receive Hiragana practice messages.");

		return user;
	}

	private async Task ProcessAnswer(long chatId, string userMessage, CancellationToken cancellationToken)
	{
		Domain.User? user = await _userRepository.GetByTelegramIdAsync(chatId);

		if (user == null)
		{
			return;
		}

		Question? question = await _questionRepository.GetOldestUnansweredQuestionAsync(user.Id);
		
		if (question == null)
			return;

		if (userMessage == question.CorrectAnswer)
		{
			question.IsAnswered = true;
			user.IncreaseStreak();
			await _userRepository.SaveChangesAsync(cancellationToken);

			Kana? kana = await _kanaRepository.GetByCharacterAsync(question.QuestionText, cancellationToken);
			if (kana == null)
			{
				return;
			}

			string message = $"Correct! The Romaji for {question.QuestionText} is {question.CorrectAnswer}.";
			if (kana.Variants != null && kana.Variants.Count > 0)
			{
				message += "\nVariants: \n";
				foreach (var variant in kana.Variants)
				{
					message += "   " + variant.Character + " is " + variant.Romaji + "\n";
				}
			}
			message += $"\n\nYour current streak is **{user.Streak}**.";
			await _botClient.SendMessage(chatId,
				message,
				ParseMode.Markdown, cancellationToken: cancellationToken, replyParameters: question.MessageId);
		}
		else
		{
			question.Attempts++;

			if (question.Attempts >= 3)
			{
				question.IsExpired = true;
				user.ResetStreak();
				await _userRepository.SaveChangesAsync(cancellationToken);

				await _botClient.SendMessage(chatId, "You've reached the maximum number of attempts. The correct answer was " + question.CorrectAnswer, cancellationToken: cancellationToken, replyParameters: question.MessageId);
				return;
			}
			await _questionRepository.SaveChangesAsync(cancellationToken);

			await _botClient.SendMessage(chatId, "Incorrect. Please try again.", cancellationToken: cancellationToken, replyParameters: question.MessageId);
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
