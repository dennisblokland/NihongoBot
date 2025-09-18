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
			return new InlineKeyboardMarkup(
			[
				[CreateMenuButton("Back", 1, messageId)]
			]);
		}

		List<List<InlineKeyboardButton>> rows = [];

		// Get all hiragana characters with their display names
		Dictionary<string, string> characterDisplayNames = Domain.User.GetCharacterDisplayNames();
		List<string> allCharacters = characterDisplayNames.Keys.ToList();

		// Group characters by category for better organization
		List<(string Category, List<string> Characters)> characterGroups =
		[
			("Basic Vowels", new List<string> { "a", "i", "u", "e", "o" }),
			("K Sounds", new List<string> { "ka", "ki", "ku", "ke", "ko" }),
			("S Sounds", new List<string> { "sa", "shi", "su", "se", "so" }),
			("T Sounds", new List<string> { "ta", "chi", "tsu", "te", "to" }),
			("N Sounds", new List<string> { "na", "ni", "nu", "ne", "no" }),
			("H Sounds", new List<string> { "ha", "hi", "fu", "he", "ho" }),
			("M Sounds", new List<string> { "ma", "mi", "mu", "me", "mo" }),
			("Y Sounds", new List<string> { "ya", "yu", "yo" }),
			("R Sounds", new List<string> { "ra", "ri", "ru", "re", "ro" }),
			("W/N Sounds", new List<string> { "wa", "wo", "n" }),
			("Combination Sounds", new List<string> { 
				"kya", "kyu", "kyo", "sha", "shu", "sho", "cha", "chu", "cho",
				"nya", "nyu", "nyo", "hya", "hyu", "hyo", "mya", "myu", "myo",
				"rya", "ryu", "ryo"
			})
		];

		foreach ((string Category, List<string> Characters) group in characterGroups)
		{
			// Add category header that can toggle the whole category
			bool allCategoryEnabled = group.Characters.All(character => user.IsCharacterEnabled(character));
			string categoryToggleText = allCategoryEnabled ? $"✅ 📝 {group.Category}" : $"❌ 📝 {group.Category}";
			string categoryToggleValue = allCategoryEnabled ? "false" : "true";
			
			rows.Add(
			[
				CreateCategoryToggleButton(group.Category, group.Characters, categoryToggleValue, messageId, categoryToggleText)
			]);

			// Add characters in this group (2 per row to avoid too wide buttons)
			for (int i = 0; i < group.Characters.Count; i += 2)
			{
				List<InlineKeyboardButton> row = [];
				
				for (int j = i; j < Math.Min(i + 2, group.Characters.Count); j++)
				{
					string character = group.Characters[j];
					string label = characterDisplayNames[character];
					bool isEnabled = user.IsCharacterEnabled(character);
					string buttonText = isEnabled ? $"✅ {label}" : $"❌ {label}";
					string toggleValue = isEnabled ? "false" : "true";
					
					row.Add(CreateCharacterToggleButton(character, toggleValue, messageId, buttonText));
				}
				
				rows.Add(row);
			}
		}

		// Add "Select All" and "Deselect All" buttons
		rows.Add(
		[
			CreateSpecialActionButton("select_all", messageId, "✅ Enable All"),
			CreateSpecialActionButton("deselect_all", messageId, "❌ Disable All")
		]);

		// Add back button
		rows.Add(
		[
			CreateMenuButton("Back", 1, messageId)
		]);

		return new InlineKeyboardMarkup(rows);
	}

	private InlineKeyboardButton CreateCharacterToggleButton(string character, string value, int messageId, string buttonText)
	{
		SettingsOptionCallbackData data = new SettingsOptionCallbackData(SettingType.CharacterSelection, $"{character}:{value}") { MessageId = messageId };
		return InlineKeyboardButton.WithCallbackData(buttonText, Serialize(data));
	}

	private InlineKeyboardButton CreateSpecialActionButton(string action, int messageId, string buttonText)
	{
		SettingsOptionCallbackData data = new SettingsOptionCallbackData(SettingType.CharacterSelection, $"special:{action}") { MessageId = messageId };
		return InlineKeyboardButton.WithCallbackData(buttonText, Serialize(data));
	}

	private InlineKeyboardButton CreateCategoryToggleButton(string category, List<string> characters, string value, int messageId, string buttonText)
	{
		// Create a special callback for category toggle with the character list
		string charactersJson = System.Text.Json.JsonSerializer.Serialize(characters);
		SettingsOptionCallbackData data = new SettingsOptionCallbackData(SettingType.CharacterSelection, $"category:{category}:{value}:{charactersJson}") { MessageId = messageId };
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
