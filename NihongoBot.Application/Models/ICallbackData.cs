using NihongoBot.Application.Enums;

namespace NihongoBot.Application.Models;

public interface ICallbackData
{
	public CallBackType Type { get; }
}
