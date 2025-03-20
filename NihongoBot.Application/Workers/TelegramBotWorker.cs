using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NihongoBot.Application.Services;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

public class TelegramBotWorker : IHostedService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<TelegramBotWorker> _logger;
    private readonly BotService _botService;
    private readonly CancellationTokenSource _cts = new();

    public TelegramBotWorker(ITelegramBotClient botClient, ILogger<TelegramBotWorker> logger, BotService botService)
    {
        _botClient = botClient;
        _logger = logger;
        _botService = botService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Telegram bot is starting...");

		ReceiverOptions receiverOptions = new()
		{
            AllowedUpdates = [], // Receive all update types
        };

        _botClient.StartReceiving(
            updateHandler: _botService.HandleUpdateAsync,
            errorHandler: _botService.HandleErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: _cts.Token
        );
	 	await _botClient.SetMyCommands(Commands);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Telegram bot is stopping...");
        _cts.Cancel();
        return Task.CompletedTask;
    }

	  public static readonly List<BotCommand> Commands =
    [
        new BotCommand { Command = "start", Description = "Start interacting with NihongoBot" },
        new BotCommand { Command = "streak", Description = "Check your current streak" },
        new BotCommand { Command = "resetstreak", Description =  "Reset your current streak" },
    ];

}
