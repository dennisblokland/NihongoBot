using NihongoBot.Application.Enums;

namespace NihongoBot.Application.Models;

public class SettingsMenuCallbackData : AbstractCallbackData
{
	public SettingsMenuCallbackData(int menuLevel = 1, string? selectedOption = null)
	{
		MenuLevel = menuLevel;
		SelectedOption = selectedOption;
	}

	public override CallBackType Type  => CallBackType.SettingsMenu;
    public int MenuLevel { get; set; }
    public string? SelectedOption { get; set; }
}
