using NihongoBot.Application.Enums;

namespace NihongoBot.Application.Models;

public class SettingsMenuCallbackData : ICallbackData
{
    public CallBackType Type { get; } = CallBackType.SettingsMenu;
    public int MenuLevel { get; set; }
    public string? SelectedOption { get; set; }
}
