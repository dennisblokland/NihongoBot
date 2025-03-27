using NihongoBot.Application.Models;

namespace NihongoBot.Application.Handlers;

public interface ITelegramCallbackHandler<in T> where T : ICallbackData
{
	Task HandleAsync(long chatId, T callbackData, CancellationToken cancellationToken);
}
