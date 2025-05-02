using NihongoBot.Application.Enums;

namespace NihongoBot.Application.Models;

public class SettingsOptionCallbackData : AbstractCallbackData
{
	internal SettingsOptionCallbackData() 
	{
		Value = string.Empty;
	}
	public SettingsOptionCallbackData(SettingType setting, string value)
	{
		Setting = setting;
		Value = value;
	}

	public override CallBackType Type => CallBackType.SettingsOption;
    public SettingType Setting { get; set; }
    public string Value { get; set; }
}
