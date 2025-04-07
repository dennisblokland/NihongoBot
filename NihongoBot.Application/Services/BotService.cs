using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using NihongoBot.Application.Models;
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
	private readonly ILogger<BotService> _logger;
	private readonly CommandDispatcher _commandDispatcher;
	private readonly CallbackDispatcher _callbackDispatcher;
	private readonly HiraganaService _hiraganaService;

	public BotService(
		IUserRepository userRepository,
		IQuestionRepository questionRepository,
		IKanaRepository kanaRepository,
		ITelegramBotClient botClient,
		ILogger<BotService> logger,
		CommandDispatcher commandDispatcher,
		CallbackDispatcher callbackDispatcher,
		HiraganaService hiraganaService)
	{
		_userRepository = userRepository;
		_botClient = botClient;
		_questionRepository = questionRepository;
		_kanaRepository = kanaRepository;
		_logger = logger;
		_commandDispatcher = commandDispatcher;
		_callbackDispatcher = callbackDispatcher;
		_hiraganaService = hiraganaService;
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
		else if (update.Type == UpdateType.CallbackQuery)
		{
			await HandleCallback(update, cancellationToken);
		}
	}

	private async Task HandleCommand(long chatId, string command, CancellationToken cancellationToken)
	{
		string[] args = command.Split(' ');
		string commandName = args[0].Substring(1); // Remove the leading '/'
		if (commandName == "settings")
		{
			await ShowSettingsMenu(chatId, cancellationToken);
		}
		else
		{
			await _commandDispatcher.DispatchAsync(chatId, commandName, args.Skip(1).ToArray(), cancellationToken);
		}
	}

	private async Task ShowSettingsMenu(long chatId, CancellationToken cancellationToken)
	{
		SettingsMenuCallbackData callbackData = new()
		{
			MenuLevel = 1
		};
		await _callbackDispatcher.DispatchAsync(chatId, callbackData, cancellationToken);
	}

	private async Task HandleCallback(Update update, CancellationToken cancellationToken)
	{
		if (update.CallbackQuery?.Message?.Chat?.Id == null)
		{
				_logger.LogWarning("CallbackQuery or its Message/Chat is null.");
				return;
		}
		long chatId = update.CallbackQuery.Message.Chat.Id;
		ICallbackData data = JsonConvert.DeserializeObject<ICallbackData>(update.CallbackQuery?.Data, new CallbackDataConverter());
		await _callbackDispatcher.DispatchAsync(chatId, data, cancellationToken);
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
				foreach (KanaVariant variant in kana.Variants)
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
		string errorMessage = exception switch
		{
			ApiRequestException apiRequestException => $"Telegram API Error: [{apiRequestException.ErrorCode}] {apiRequestException.Message}",
			_ => exception.ToString()
		};

		_logger.LogError(errorMessage);
		return Task.CompletedTask;
	}
}
