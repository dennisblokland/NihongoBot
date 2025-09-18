using NihongoBot.Application.Enums;
using NihongoBot.Application.Models;
using NihongoBot.Application.Services;
using NihongoBot.Domain.Interfaces.Repositories;

using Telegram.Bot;

namespace NihongoBot.Application.Handlers;

public class SettingsOptionCallbackHandler : ITelegramCallbackHandler<SettingsOptionCallbackData>
{
    private readonly ITelegramBotClient _botClient;
    private readonly IUserRepository _userRepository;
    private readonly HangfireSchedulerService _schedulerService;

    public SettingsOptionCallbackHandler(ITelegramBotClient botClient, IUserRepository userRepository, HangfireSchedulerService schedulerService)
    {
        _botClient = botClient;
        _userRepository = userRepository;
        _schedulerService = schedulerService;
    }

    public async Task HandleAsync(long chatId, SettingsOptionCallbackData callbackData, CancellationToken cancellationToken)
    {
		Domain.User? user = await _userRepository.GetByTelegramIdAsync(chatId, cancellationToken);
        if (user == null)
        {
            await _botClient.SendMessage(chatId, "User not found.", cancellationToken: cancellationToken);
            return;
        }

        switch (callbackData.Setting)
        {
            case SettingType.QuestionsPerDay:
                user.QuestionsPerDay = int.Parse(callbackData.Value);
                _schedulerService.ScheduleHiraganaJobsForUser(user);
                break;
            case SettingType.WordOfTheDay:
                user.WordOfTheDayEnabled = bool.Parse(callbackData.Value);
                break;
            case SettingType.CharacterSelection:
                // Parse character and enabled state from format "character:true/false"
                string[] parts = callbackData.Value.Split(':');
                if (parts.Length == 2)
                {
                    string character = parts[0];
                    bool enabled = bool.Parse(parts[1]);
                    user.UpdateCharacterSelection(character, enabled);
                    
                    // Save changes and refresh the character selection menu
                    await _userRepository.SaveChangesAsync(cancellationToken);
                    
                    // Redirect back to character selection menu to show updated state
                    SettingsMenuCallbackData redirectData = new SettingsMenuCallbackData(4) { MessageId = callbackData.MessageId };
                    SettingsMenuCallbackHandler menuHandler = new SettingsMenuCallbackHandler(_botClient, _userRepository);
                    await menuHandler.HandleAsync(chatId, redirectData, cancellationToken);
                    return;
                }
                break;
            default:
                await _botClient.SendMessage(chatId, "Invalid setting.", cancellationToken: cancellationToken);
                return;
        }

        await _userRepository.SaveChangesAsync(cancellationToken);
		await _botClient.EditMessageText(chatId, callbackData.MessageId!.Value, "Settings updated successfully.", cancellationToken: cancellationToken);

    }
}
