using System.Text.Json;

using NihongoBot.Application.Enums;
using NihongoBot.Application.Models;
using NihongoBot.Domain.Interfaces.Repositories;

using Telegram.Bot;
using Telegram.Bot.Types;
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
		int? messageId = callbackData.MessageId;
		if (!callbackData.MessageId.HasValue)
		{
			//send dummy message to get a message id to edit later
			Message message = await _botClient.SendMessage(chatId, "Settings Menu", cancellationToken: cancellationToken);
			messageId = message.MessageId;
		}

		InlineKeyboardMarkup inlineKeyboard = callbackData.MenuLevel switch
		{
			1 => new InlineKeyboardMarkup(
			[
				[CreateMenuButton("Questions per day", 2 , messageId!.Value)],
				[CreateMenuButton("Word of the day", 3,  messageId!.Value)],
				[CreateMenuButton("Character selection", 4,  messageId!.Value)]
			]),
			2 => new InlineKeyboardMarkup(Enumerable.Range(1, 6)
				.Select(i => new[] { CreateOptionButton(SettingType.QuestionsPerDay, i.ToString(), messageId!.Value) })
				.ToList()),
			3 => new InlineKeyboardMarkup(
			[
				[CreateOptionButton(SettingType.WordOfTheDay, "True",  messageId!.Value, "Enable" )],
				[CreateOptionButton(SettingType.WordOfTheDay, "False", messageId!.Value, "Disable")]
			]),
			4 => await CreateCharacterSelectionMenu(chatId, messageId!.Value, cancellationToken),
			_ => new InlineKeyboardMarkup(new[]
			{
				new[] { CreateMenuButton("Back", 1, messageId!.Value) }
			})
		};

		await _botClient.EditMessageText(chatId, messageId!.Value, "Settings Menu", replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
	}
	private InlineKeyboardButton CreateMenuButton(string text, int nextMenuLevel, int messageId)
	{
		SettingsMenuCallbackData data = new SettingsMenuCallbackData(nextMenuLevel) { MessageId = messageId };
		return InlineKeyboardButton.WithCallbackData(text, Serialize(data));
	}

	private InlineKeyboardButton CreateOptionButton(SettingType type, string value, int messageId, string? textOverride = null)
	{
		SettingsOptionCallbackData data = new SettingsOptionCallbackData(type, value) { MessageId = messageId };
		return InlineKeyboardButton.WithCallbackData(textOverride ?? value, Serialize(data));
	}

	private async Task<InlineKeyboardMarkup> CreateCharacterSelectionMenu(long chatId, int messageId, CancellationToken cancellationToken)
	{
		Domain.User? user = await _userRepository.GetByTelegramIdAsync(chatId, cancellationToken);
		if (user == null)
		{
			return new InlineKeyboardMarkup(new[]
			{
				new[] { CreateMenuButton("Back", 1, messageId) }
			});
		}

		List<List<InlineKeyboardButton>> rows = new List<List<InlineKeyboardButton>>();

		// Create toggle buttons for each character
		string[] characters = { "ka", "ki", "ku", "ke", "ko" };
		string[] characterLabels = { "Ka (か)", "Ki (き)", "Ku (く)", "Ke (け)", "Ko (こ)" };
		bool[] enabledStates = { user.KaEnabled, user.KiEnabled, user.KuEnabled, user.KeEnabled, user.KoEnabled };

		for (int i = 0; i < characters.Length; i++)
		{
			string character = characters[i];
			string label = characterLabels[i];
			bool isEnabled = enabledStates[i];
			string buttonText = isEnabled ? $"✅ {label}" : $"❌ {label}";
			string toggleValue = isEnabled ? "false" : "true";
			
			rows.Add(new List<InlineKeyboardButton>
			{
				CreateCharacterToggleButton(character, toggleValue, messageId, buttonText)
			});
		}

		// Add back button
		rows.Add(new List<InlineKeyboardButton>
		{
			CreateMenuButton("Back", 1, messageId)
		});

		return new InlineKeyboardMarkup(rows);
	}

	private InlineKeyboardButton CreateCharacterToggleButton(string character, string value, int messageId, string buttonText)
	{
		SettingsOptionCallbackData data = new SettingsOptionCallbackData(SettingType.CharacterSelection, $"{character}:{value}") { MessageId = messageId };
		return InlineKeyboardButton.WithCallbackData(buttonText, Serialize(data));
	}

	private static string Serialize(ICallbackData data)
	{
		return data switch
		{
			SettingsMenuCallbackData menuData => $"{(int)CallBackType.SettingsMenu}|{menuData.MenuLevel}|{menuData.MessageId ?? 0}",
			SettingsOptionCallbackData optionData => $"{(int)CallBackType.SettingsOption}|{(int)optionData.Setting}|{optionData.Value}|{optionData.MessageId ?? 0}",
			_ => JsonSerializer.Serialize((object)data) // Fallback for other types
		};
	}
}
