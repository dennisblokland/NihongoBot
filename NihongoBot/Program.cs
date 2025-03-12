using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Quartz;
using Quartz.Impl;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Dapper;
using SkiaSharp;
using Persistence;
using Microsoft.EntityFrameworkCore;

class Program
{
    private static readonly string BotToken;
    public static readonly TelegramBotClient BotClient;
    public static readonly List<HiraganaEntry> HiraganaList;
    private static IScheduler Scheduler;

    public static readonly AppDbContext DbContext;

    static Program()
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        BotToken = config["TelegramBotToken"] ?? Environment.GetEnvironmentVariable("TelegramBotToken");
        BotClient = new TelegramBotClient(BotToken);

        // Load Hiragana from JSON
        HiraganaList = LoadHiraganaFromJson("hiragana.json");

        DbContext = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(config.GetConnectionString("NihongoBot"))
            .Options);
    }

    static async Task Main()
    {
        Console.WriteLine("Starting NihongoBot...");
        InitializeDatabase();
        // Schedule the daily tasks
        await ScheduleDailyTasks();

        // Setup commands to the bot to build the user's toolbox.
        await BotClient.SetMyCommands(Commands);

        // Start receiving messages
        BotClient.StartReceiving(UpdateHandler, ErrorHandler);

        Console.WriteLine("Bot is running...");
        Console.ReadLine();
    }

    private static void InitializeDatabase()
    {
        using SqliteConnection connection = new("Data Source=nihongoBot.db");
        connection.Execute(@"CREATE TABLE IF NOT EXISTS Users (
            Id INTEGER PRIMARY KEY,
            TelegramId INTEGER UNIQUE,
            Streak INTEGER DEFAULT 0
        );");

        // Create a trigger log table
        connection.Execute(@"CREATE TABLE IF NOT EXISTS TriggerLog (
            Id INTEGER PRIMARY KEY,
            TriggerTime TEXT
        );");

        // Create a table to store if the user has answered the Hiragana question
        connection.Execute(@"CREATE TABLE IF NOT EXISTS HiraganaAnswers (
            Id INTEGER PRIMARY KEY,
            TelegramId INTEGER,
            Correct INTEGER DEFAULT 0,
            Character TEXT,
            FOREIGN KEY (TelegramId) REFERENCES Users(TelegramId)
        );");
        connection.Close();
    }

    private static async Task UpdateHandler(ITelegramBotClient bot, Update update, CancellationToken token)
    {
        using SqliteConnection connection = new("Data Source=nihongoBot.db");

        if (update.Type == UpdateType.Message && update.Message?.Text != null)
        {
            long chatId = update.Message.Chat.Id;
            string userMessage = update.Message.Text.Trim().ToLower();
            if (userMessage.StartsWith("/"))
            {
                await HandleCommand(bot, chatId, userMessage, connection);
            }
            else // For now, if it's not a command, then it might be a answer to a question I've asked. (Can be made into a session command handler.)
            {
                await ProcessAnswer(bot, connection, chatId, userMessage);
            }
        }
        connection.Close();
    }

    private static async Task ProcessAnswer(ITelegramBotClient bot, SqliteConnection connection, long chatId, string userMessage)
    {
        HiraganaAnswer lastHiragana = connection.QueryFirstOrDefault<HiraganaAnswer>(@"SELECT id, Character FROM HiraganaAnswers WHERE TelegramId = @ChatId ORDER BY Id DESC LIMIT 1;", new { ChatId = chatId });

        HiraganaEntry? hiragana = HiraganaList.Find(h => h.Character == lastHiragana.Character && h.Romaji == userMessage.ToLower());
        if (hiragana != null)
        {
            connection.Execute("UPDATE HiraganaAnswers SET Correct = 1 WHERE Id = @id", new { id = lastHiragana.Id });
            connection.Execute("UPDATE Users SET Streak = Streak + 1 WHERE TelegramId = @ChatId;", new { ChatId = chatId });
            //send the a message to the user with the correct answer possible variations and the streak
            int streak = connection.QueryFirstOrDefault<int>("SELECT Streak FROM Users WHERE TelegramId = @ChatId;", new { ChatId = chatId });
            string message = $"Correct! The Romaji for {hiragana.Character} is {hiragana.Romaji}.\n";
            if (hiragana.Variants != null && hiragana.Variants.Count > 0)
            {
                message += "Variants: \n";
                foreach (var variant in hiragana.Variants)
                {
                    message += "   " + variant.Character + " is " + variant.Romaji + "\n";
                }
            }
            message += $"Your current streak is **{streak}**.";
            await bot.SendMessage(chatId, message, ParseMode.Markdown);
        }
        else
        {
            await bot.SendMessage(chatId, "Incorrect. Please try again.");
        }
    }

    private static Task ErrorHandler(ITelegramBotClient bot, Exception exception, CancellationToken token)
    {
        Console.WriteLine("Error: " + exception.Message);
        return Task.CompletedTask;
    }

    private static async Task RegisterUser(long chatId, string username)
    {
        DbContext.Users.Add(new NihongoBot.Domain.User(chatId, username));
        await DbContext.SaveChangesAsync();
    }

    private static async Task ScheduleDailyTasks()
    {
        Scheduler = await new StdSchedulerFactory().GetScheduler();
        await Scheduler.Start();

        IJobDetail job = JobBuilder.Create<HiraganaJob>().Build();
        var triggers = TriggerGenerator.GetNextTriggers(8, 21); // Generate based on last trigger time

        await Scheduler.ScheduleJobs(new Dictionary<IJobDetail, IReadOnlyCollection<ITrigger>>
        {
            { job, triggers }
        }, true);

    }

    private static List<HiraganaEntry> LoadHiraganaFromJson(string filePath)
    {
        if (!File.Exists(filePath)) throw new FileNotFoundException("Hiragana JSON file not found.");
        string jsonString = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<List<HiraganaEntry>>(jsonString) ?? [];
    }

    private static async Task HandleCommand(ITelegramBotClient bot, long chatId, string command, SqliteConnection connection)
    {
        ChatFullInfo chat = await bot.GetChat(chatId);
        switch (command)
        {
            case "/start":
                await RegisterUser(chatId, chat.Username);
                await bot.SendMessage(chatId, "Welcome to NihongoBot! You're now registered to receive Hiragana practice messages.");
                break;
            case "/streak":
                int streak = connection.QueryFirstOrDefault<int>("SELECT Streak FROM Users WHERE TelegramId = @ChatId;", new { ChatId = chatId });
                await bot.SendMessage(chatId, $"Your current streak is {streak}.");
                break;
            default:
                await bot.SendMessage(chatId, "Command not recognized.");
                break;
        }
    }

    public static readonly List<BotCommand> Commands =
    [
        new BotCommand { Command = "start", Description = "Start interacting with NihongoBot" },
        new BotCommand { Command = "register", Description = "Register to receive Hiragana practice messages" },
        new BotCommand { Command = "streak", Description = "Check your current streak" },
    ];

    public static byte[] RenderCharacterToImage(string character)
    {
        int width = 250;
        int height = 250;
        int fontSize = 126;
        if (character.Length > 1)
            fontSize = 100;

        using SKBitmap bitmap = new(width, height);
        using SKCanvas canvas = new(bitmap);
        using SKPaint paint = new()
        {
            Color = SKColors.Black,
            IsAntialias = true
        };

        string fontPath = "fonts/NotoSansJP-Regular.ttf";
        //check if file exists
        if (!File.Exists(fontPath))
        {
            Console.WriteLine("Font file not found.");
            return Array.Empty<byte>();
        }
        using SKTypeface typeface = SKTypeface.FromFile(fontPath);
        using SKFont font = new()
        {
            Size = fontSize,
            Typeface = typeface
        };

        canvas.Clear(SKColors.White);

        // Measure the string to center it
        SKRect textBounds = new();
        font.MeasureText(character, out textBounds);
        float x = (width - textBounds.Width) / 2 - 10;
        float y = (height + textBounds.Height) / 2;

        canvas.DrawText(character, x, y, SKTextAlign.Left, font, paint);

        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}
