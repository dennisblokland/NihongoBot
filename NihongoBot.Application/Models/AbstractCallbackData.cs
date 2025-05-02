using NihongoBot.Application.Enums;

namespace NihongoBot.Application.Models;

public abstract class AbstractCallbackData : ICallbackData
{
	public virtual CallBackType Type { get; } = CallBackType.Unknown;

	public int? MessageId { get; set; }
}
