using Quartz.Impl.Triggers;
using Quartz;
using Microsoft.Data.Sqlite;

public static class TriggerGenerator
{
    public static List<ITrigger> GetNextTriggers(int intialMinHour, int maxHour)
    {
        int minHour = intialMinHour;
        DateTime? lastTriggerTime = GetLastTriggerTime();

        // Check if the last trigger was today
        if (lastTriggerTime != null && lastTriggerTime?.Date == DateTime.UtcNow.Date)
        {
            lastTriggerTime = lastTriggerTime?.AddDays(1);
        }

        DateTime date = lastTriggerTime ?? DateTime.UtcNow;

        Random random = new();


        //if the date is today, set the minimum hour to the current hour
        if (date.Date == DateTime.UtcNow.Date)
        {
            minHour = DateTime.UtcNow.Hour;
        }

        //if minHour is greater than maxHour, set minHour to  the original minHour and set the date to tomorrow
        if (minHour >= maxHour)
        {
            minHour = intialMinHour;
            date = date.AddDays(1);
        }

        int firstHour = random.Next(minHour, maxHour + 1);
        int firstMinute = random.Next(0, 60);

        int secondHour, secondMinute;
        do
        {
            secondHour = random.Next(minHour, maxHour + 1);
            secondMinute = random.Next(0, 60);
        } while (firstHour == secondHour && firstMinute == secondMinute); // Ensure unique times

        Console.WriteLine($"Last Trigger: {lastTriggerTime}");
        Console.WriteLine($"Next Trigger 1: {new DateTime(date.Year, date.Month, date.Day, firstHour, firstMinute, 0)} UTC");
        Console.WriteLine($"Next Trigger 2: {new DateTime(date.Year, date.Month, date.Day, secondHour, secondMinute, 0)} UTC");

        // Convert times to DateTime
        DateTime firstTriggerTime = new DateTime(date.Year, date.Month, date.Day, firstHour, firstMinute, 0);
        DateTime secondTriggerTime = new DateTime(date.Year, date.Month, date.Day, secondHour, secondMinute, 0);

        // Create triggers using SimpleTriggerImpl
        SimpleTriggerImpl trigger1 = new()
        {
            Name = "trigger1",
            Group = "group1",
            StartTimeUtc = firstTriggerTime,
            RepeatCount = 0, // Fire once
        };

        SimpleTriggerImpl trigger2 = new()
        {
            Name = "trigger2",
            Group = "group1",
            StartTimeUtc = secondTriggerTime,
            RepeatCount = 0, // Fire once
        };

        return [trigger1, trigger2];
    }

    private static DateTime? GetLastTriggerTime()
    {
        using SqliteConnection connection = new("Data Source=nihongoBot.db");
        connection.Open();
        string sql = "SELECT TriggerTime FROM TriggerLog ORDER BY Id DESC LIMIT 1;";
        using SqliteCommand command = new(sql, connection);
        using SqliteDataReader reader = command.ExecuteReader();
        if (reader.Read())
        {
            var result = reader.GetString(0);
            connection.Close();
            return DateTime.Parse(result);
        }

        connection.Close(); // Close the connection

        return null; // Return null if no log is found
    }
}
