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
				[CreateMenuButton("Word of the day", 3,  messageId!.Value)]
			]),
			2 => new InlineKeyboardMarkup(Enumerable.Range(1, 6)
				.Select(i => new[] { CreateOptionButton(SettingType.QuestionsPerDay, i.ToString(), messageId!.Value) })
				.ToList()),
			3 => new InlineKeyboardMarkup(
			[
				[CreateOptionButton(SettingType.WordOfTheDay, "True",  messageId!.Value, "Enable" )],
				[CreateOptionButton(SettingType.WordOfTheDay, "False", messageId!.Value, "Disable")]
			]),
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
