namespace NihongoBot.Application.Handlers;

public interface ITelegramCommandHandler
{
	Task HandleAsync(long chatId, string[] args, CancellationToken cancellationToken);
}
