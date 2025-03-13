using Microsoft.Extensions.Logging;

using NihongoBot.Application.Helpers;
using NihongoBot.Domain.Aggregates.Hiragana;
using NihongoBot.Persistence;

using Telegram.Bot;
using Telegram.Bot.Types;

using User = NihongoBot.Domain.User;

namespace NihongoBot.Application.Services;

public class HiraganaService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<HiraganaService> _logger;
    private readonly AppDbContext _dbContext;

    public HiraganaService(ITelegramBotClient botClient, ILogger<HiraganaService> logger, AppDbContext dbContext)
    {
        _botClient = botClient;
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task SendHiraganaMessage(long telegramId)
    {
        _logger.LogInformation("Sending Hiragana message at {Time}", DateTime.Now);

        Kana kana = _dbContext.Kanas.OrderBy(h => Guid.NewGuid()).FirstOrDefault();
        Dictionary<Kana, byte[]> renderedKana = [];

        byte[] imageBytes = KanaRenderer.RenderCharacterToImage(kana.Character);
        renderedKana.Add(kana, imageBytes);

        //take random hiragana character from the list
        KeyValuePair<Kana, byte[]> hiragana = renderedKana.OrderBy(h => Guid.NewGuid()).First();
        Stream stream = new MemoryStream(hiragana.Value);
        await _botClient.SendPhoto(telegramId,
        InputFile.FromStream(stream, "hiragana.png"),
        caption: $"What is the Romaji for this {hiragana.Key.Character} Hiragana character?");
    }
}
