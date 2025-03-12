using Telegram.Bot;
using Quartz;
using Microsoft.Data.Sqlite;
using Dapper;
using Telegram.Bot.Types;

public class HiraganaJob : IJob
{
	public async Task Execute(IJobExecutionContext context)
	{
		using SqliteConnection connection = new("Data Source=nihongoBot.db");

		connection.Execute("INSERT INTO TriggerLog (TriggerTime) VALUES (@time);", new { time = DateTime.UtcNow });

		Console.WriteLine("Sending Hiragana character...");

		if (Program.HiraganaList.Count == 0) return;
		Random random = new();
		HiraganaEntry hiragana = Program.HiraganaList[random.Next(Program.HiraganaList.Count)];

		IEnumerable<int> chatIds = [.. connection.Query<int>("SELECT TelegramId FROM Users")];

		foreach (int id in chatIds)
		{
			byte[] imageBytes = Program.RenderCharacterToImage(hiragana.Character);
			using MemoryStream stream = new(imageBytes);
			await Program.BotClient.SendPhotoAsync(id,
				 InputFile.FromStream(stream, "hiragana.png"),
				caption: $"What is the Romaji for this Hiragana character?");

			// check if the user has answered previous question in the HiraganaAnswers table and if not, update the streak to 0
			connection.Execute("UPDATE Users SET Streak = 0 WHERE TelegramId = @id AND NOT EXISTS (SELECT * FROM HiraganaAnswers WHERE TelegramId = @id AND Correct = 1);", new { id });

			// Insert the Hiragana character into the HiraganaAnswers table
			connection.Execute("INSERT INTO HiraganaAnswers (TelegramId, Character) VALUES (@id, @character);", new { id, character = hiragana.Character });
		}
		await RescheduleNextTriggersAsync(context.Scheduler);
		connection.Close();
	}

	private static async Task RescheduleNextTriggersAsync(IScheduler scheduler)
	{
		var triggers = TriggerGenerator.GetNextTriggers(10, 21); // Generate new triggers

		IJobDetail job = JobBuilder.Create<HiraganaJob>()
			.Build();

		await scheduler.ScheduleJobs(new Dictionary<IJobDetail, IReadOnlyCollection<ITrigger>>
		{
			{ job, triggers }
		}, true);

		Console.WriteLine("New random triggers scheduled.");
	}
}
