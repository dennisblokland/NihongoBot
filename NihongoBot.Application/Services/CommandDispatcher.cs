using Microsoft.Extensions.DependencyInjection;

using NihongoBot.Application.Handlers;

namespace NihongoBot.Application.Services
{
	public class CommandDispatcher
	{
		private readonly IServiceProvider _serviceProvider;

		public CommandDispatcher(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public async Task DispatchAsync(long chatId, string command, string[] args, CancellationToken cancellationToken)
		{
			Type? handlerType = Type.GetType($"NihongoBot.Application.Handlers.{command}CommandHandler", false, true);
			
			if (handlerType != null)
			{
				using (IServiceScope scope = _serviceProvider.CreateScope())
				{
					if (scope.ServiceProvider.GetService(handlerType) is ITelegramCommandHandler handler)
					{
						await handler.HandleAsync(chatId, args, cancellationToken);
					}
				}
			}
		}
	}
}
