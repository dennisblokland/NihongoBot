using Microsoft.Extensions.DependencyInjection;

using NihongoBot.Application.Handlers;
using NihongoBot.Application.Models;

namespace NihongoBot.Application.Services
{
	public class CallbackDispatcher
	{
		private readonly IServiceProvider _serviceProvider;

		public CallbackDispatcher(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public async Task DispatchAsync(long chatId, ICallbackData callbackData, CancellationToken cancellationToken)
		{
			string? enumName = Enum.GetName(callbackData.Type);
			if (enumName == null)
			{
				return;
			}
			Type? handlerType = Type.GetType($"NihongoBot.Application.Handlers.{enumName}CallbackHandler", false, true);

			if (handlerType != null)
			{
				using (IServiceScope scope = _serviceProvider.CreateScope())
				{
					// Resolve the handler instance.
					object? handlerInstance = scope.ServiceProvider.GetService(handlerType);
					if (handlerInstance == null)
					{
						return;
					}

					// Find the interface implemented by the handler that matches ITelegramCallbackHandler<>
					Type? interfaceType = handlerType.GetInterfaces()
						.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITelegramCallbackHandler<>));

					if (interfaceType != null)
					{
						// Get the generic type parameter T from ITelegramCallbackHandler<T>
						Type expectedCallbackType = interfaceType.GetGenericArguments()[0];

						// Check if callbackData is compatible with T.
						if (expectedCallbackType.IsAssignableFrom(callbackData.GetType()))
						{
							// Get the HandleAsync method from the interface.
							System.Reflection.MethodInfo? handleMethod = interfaceType.GetMethod("HandleAsync");
							if (handleMethod != null)
							{
								// Invoke the method. Since it returns a Task, cast and await it.
								Task task = (Task) handleMethod.Invoke(handlerInstance, [chatId, callbackData, cancellationToken]);
								await task;
							}
						}
					}
				}
			}
		}
	}
}
