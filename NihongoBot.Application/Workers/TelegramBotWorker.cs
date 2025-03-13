using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NihongoBot.Application.Services;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

public class TelegramBotWorker : IHostedService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<TelegramBotWorker> _logger;
    private readonly BotService _botService;
    private CancellationTokenSource _cts;

    public TelegramBotWorker(ITelegramBotClient botClient, ILogger<TelegramBotWorker> logger, BotService botService)
    {
        _botClient = botClient;
        _logger = logger;
        _botService = botService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Telegram bot is starting...");

        _cts = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>(), // Receive all update types
        };

        _botClient.StartReceiving(
            updateHandler: _botService.HandleUpdateAsync,
            errorHandler: _botService.HandleErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: _cts.Token
        );

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Telegram bot is stopping...");
        _cts.Cancel();
        return Task.CompletedTask;
    }
}