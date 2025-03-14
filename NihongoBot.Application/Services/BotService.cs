using Microsoft.Extensions.Logging;

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
                await HandleCommand(chatId, userMessage,cancellationToken);
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
        switch (command)
        {
            case "/start":
                await RegisterUser(chatId, chat.Username, cancellationToken);
                await _botClient.SendMessage(chatId, "Welcome to NihongoBot! You're now registered to receive Hiragana practice messages.");
                break;
            case "/streak":
                int streak = _dbContext.Users.FirstOrDefault(u => u.TelegramId == chatId)?.Streak ?? 0;
                await _botClient.SendMessage(chatId, $"Your current streak is {streak}.", cancellationToken: cancellationToken);
                break;
            case "/test":
                await _hiraganaService.SendHiraganaMessage(chatId);
                break;
            default:
                await _botClient.SendMessage(chatId, "Command not recognized.", cancellationToken: cancellationToken);
                break;
        }
    }

    private async Task RegisterUser(long chatId, string username, CancellationToken cancellationToken)
    {
        _dbContext.Users.Add(new NihongoBot.Domain.User(chatId, username));
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

	private static async Task ProcessAnswer(long chatId, string userMessage, CancellationToken cancellationToken)
    {
        // HiraganaAnswer lastHiragana = connection.QueryFirstOrDefault<HiraganaAnswer>(@"SELECT id, Character FROM HiraganaAnswers WHERE TelegramId = @ChatId ORDER BY Id DESC LIMIT 1;", new { ChatId = chatId });

        // HiraganaEntry? hiragana = HiraganaList.Find(h => h.Character == lastHiragana.Character && h.Romaji == userMessage.ToLower());
        // if (hiragana != null)
        // {
        //     connection.Execute("UPDATE HiraganaAnswers SET Correct = 1 WHERE Id = @id", new { id = lastHiragana.Id });
        //     connection.Execute("UPDATE Users SET Streak = Streak + 1 WHERE TelegramId = @ChatId;", new { ChatId = chatId });
        //     //send the a message to the user with the correct answer possible variations and the streak
        //     int streak = connection.QueryFirstOrDefault<int>("SELECT Streak FROM Users WHERE TelegramId = @ChatId;", new { ChatId = chatId });
        //     string message = $"Correct! The Romaji for {hiragana.Character} is {hiragana.Romaji}.\n";
        //     if (hiragana.Variants != null && hiragana.Variants.Count > 0)
        //     {
        //         message += "Variants: \n";
        //         foreach (var variant in hiragana.Variants)
        //         {
        //             message += "   " + variant.Character + " is " + variant.Romaji + "\n";
        //         }
        //     }
        //     message += $"Your current streak is **{streak}**.";
        //     await bot.SendMessage(chatId, message, ParseMode.Markdown);
        // }
        // else
        // {
        //     await bot.SendMessage(chatId, "Incorrect. Please try again.");
        // }
    }

    private string ProcessMessage(string input)
    {
        return input.ToLower() switch
        {
            "/start" => "Hello! I'm NihongoBot. I'll send you Hiragana characters to learn!",
            "/help" => "You can reply with the correct Romaji for Hiragana characters!",
            _ => "I don't understand that command. Try /help."
        };
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
