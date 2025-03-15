namespace NihongoBot.Application.Helpers
{
	public static class TimeGenerator
	{
		public static List<TimeOnly> GetRandomTimes(TimeOnly start, TimeOnly end, int count)
		{
			List<TimeOnly> result = new();
			Random rand = new();

			// Generate a list of all possible 15-minute intervals
			List<TimeOnly> possibleTimes = new();
			TimeOnly current = new(start.Hour, (start.Minute / 15) * 15); // Round down to nearest 15-min mark

			while (current <= end)
			{
				possibleTimes.Add(current);
				current = current.AddMinutes(15);
			}

			if (count >= possibleTimes.Count)
			{
				return possibleTimes; // Return all possible times if count exceeds available slots
			}

			// Divide the range into `count` segments
			int segmentSize = possibleTimes.Count / count;

			for (int i = 0; i < count; i++)
			{
				// Pick a random time within each segment
				int minIndex = i * segmentSize;
				int maxIndex = Math.Min((i + 1) * segmentSize, possibleTimes.Count) - 1;

				if (minIndex <= maxIndex)
				{
					int randomIndex = rand.Next(minIndex, maxIndex + 1);
					result.Add(possibleTimes[randomIndex]);
				}
			}

			result.Sort(); // Sort times in ascending order
			return result;
		}
	}
}
