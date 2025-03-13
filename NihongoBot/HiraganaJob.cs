using Telegram.Bot;
using Quartz;
using Telegram.Bot.Types;
using NihongoBot.Domain.Enums;
using NihongoBot.Domain.Aggregates.Hiragana;

public class HiraganaJob : IJob
{
	public async Task Execute(IJobExecutionContext context)
	{

		Console.WriteLine("Sending Hiragana character...");

		if (Program.HiraganaList.Count == 0) return;
		Random random = new();
		Kana hiragana = Program.DbContext.Kanas
			.Where(k => k.Type == KanaType.Hiragana)
			.OrderBy(k => random.Next())
			.First();

		IEnumerable<long> chatIds = Program.DbContext.Users.Select(u => u.TelegramId);

		foreach (long id in chatIds)
		{
			byte[] imageBytes = Program.RenderCharacterToImage(hiragana.Character);
			using MemoryStream stream = new(imageBytes);
			await Program.BotClient.SendPhotoAsync(id,
				 InputFile.FromStream(stream, "hiragana.png"),
				caption: $"What is the Romaji for this Hiragana character?");
		}
		await RescheduleNextTriggersAsync(context.Scheduler);

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
