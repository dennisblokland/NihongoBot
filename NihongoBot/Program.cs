using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Quartz;
using Quartz.Impl;
using Microsoft.Data.Sqlite;
using Dapper;
using Microsoft.Extensions.Configuration;
using Quartz.Impl.Triggers;

class Program
{
    private static readonly string BotToken;
    public static readonly TelegramBotClient BotClient;
    public static readonly List<HiraganaEntry> HiraganaList;
    private static IScheduler Scheduler;

    static Program()
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        BotToken = config["TelegramBotToken"];
        BotClient = new TelegramBotClient(BotToken);

        // Load Hiragana from JSON
        HiraganaList = LoadHiraganaFromJson("hiragana.json");
    }

    static async Task Main()
    {
        Console.WriteLine("Starting NihongoBot...");

        // Database setup
        InitializeDatabase();

        // Schedule the daily tasks
        await ScheduleDailyTasks();

        // Start receiving messages
        BotClient.StartReceiving(UpdateHandler, ErrorHandler);
        Console.WriteLine("Bot is running...");
        Console.ReadLine();
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
                if (userMessage == "/start")
                {
                    await bot.SendMessage(chatId, "Welcome to NihongoBot! Type /register to start receiving Hiragana practice messages.");
                    return;
                }
                if (userMessage == "/register")
                {
                    RegisterUser(chatId, connection);
                    await bot.SendMessage(chatId, "You have been registered! You will now receive Hiragana practice messages.");
                    return;
                }
                if (userMessage == "/streak")
                {
                    int streak = connection.QueryFirstOrDefault<int>("SELECT Streak FROM Users WHERE TelegramId = @ChatId;", new { ChatId = chatId });
                    await bot.SendMessage(chatId, $"Your current streak is {streak}.");
                    return;
                }
                // send a message to the user if the command is not recognized
                await bot.SendMessage(chatId, "Command not recognized.");
            }

            // handle user answers
            //get last Hiragana asked
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
                    message += "Variants: " + string.Join(", ", hiragana.Variants) + "\n";
                }
                message += $"Your current streak is {streak}.";
                await bot.SendMessage(chatId, message);
            }
            else
            {
                await bot.SendMessage(chatId, "Incorrect. Please try again.");
            }
        }
        connection.Close();
    }

    private static Task ErrorHandler(ITelegramBotClient bot, Exception exception, CancellationToken token)
    {
        Console.WriteLine("Error: " + exception.Message);
        return Task.CompletedTask;
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

    private static void RegisterUser(long chatId, SqliteConnection connection)
    {
        connection.Execute("INSERT OR IGNORE INTO Users (TelegramId, Streak) VALUES (@ChatId, 0);", new { ChatId = chatId });
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
}
