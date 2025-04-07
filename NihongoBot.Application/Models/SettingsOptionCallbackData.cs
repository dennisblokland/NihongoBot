using NihongoBot.Application.Enums;

namespace NihongoBot.Application.Models;

public class SettingsOptionCallbackData : ICallbackData
{
    public CallBackType Type { get; } = CallBackType.SettingsOption;
    public string Setting { get; set; }
    public string Value { get; set; }
}
