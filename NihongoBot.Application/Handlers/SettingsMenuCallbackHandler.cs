using NihongoBot.Application.Models;
using NihongoBot.Application.Services;
using NihongoBot.Domain.Interfaces.Repositories;

using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace NihongoBot.Application.Handlers;

public class SettingsMenuCallbackHandler : ITelegramCallbackHandler<SettingsMenuCallbackData>
{
    private readonly ITelegramBotClient _botClient;
    private readonly IUserRepository _userRepository;

    public SettingsMenuCallbackHandler(ITelegramBotClient botClient, IUserRepository userRepository)
    {
        _botClient = botClient;
        _userRepository = userRepository;
    }

    public async Task HandleAsync(long chatId, SettingsMenuCallbackData callbackData, CancellationToken cancellationToken)
    {
        Domain.User? user = await _userRepository.GetByTelegramIdAsync(chatId, cancellationToken);
        if (user == null)
        {
            await _botClient.SendMessage(chatId, "User not found.", cancellationToken: cancellationToken);
            return;
        }

        InlineKeyboardMarkup inlineKeyboard = callbackData.MenuLevel switch
        {
            1 => new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("Questions per day", "settings:questions_per_day") },
                new[] { InlineKeyboardButton.WithCallbackData("Word of the day", "settings:word_of_the_day") }
            }),
            2 => new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("1", "settings:questions_per_day:1") },
                new[] { InlineKeyboardButton.WithCallbackData("2", "settings:questions_per_day:2") },
                new[] { InlineKeyboardButton.WithCallbackData("3", "settings:questions_per_day:3") },
                new[] { InlineKeyboardButton.WithCallbackData("4", "settings:questions_per_day:4") },
                new[] { InlineKeyboardButton.WithCallbackData("5", "settings:questions_per_day:5") },
                new[] { InlineKeyboardButton.WithCallbackData("6", "settings:questions_per_day:6") }
            }),
            3 => new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("Enable", "settings:word_of_the_day:enable") },
                new[] { InlineKeyboardButton.WithCallbackData("Disable", "settings:word_of_the_day:disable") }
            }),
            _ => new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("Back", "settings:back") }
            })
        };

        await _botClient.SendMessage(chatId, "Settings Menu", replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
    }
}
