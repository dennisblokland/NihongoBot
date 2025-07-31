
using Microsoft.EntityFrameworkCore;

using NihongoBot.Domain.Aggregates.Kana;
using NihongoBot.Domain.Enums;
using NihongoBot.Domain.Interfaces.Repositories;

namespace NihongoBot.Infrastructure.Repositories;

public class KanaRepository(IServiceProvider serviceProvider) : AbstractDomainRepository<Kana, int>(serviceProvider), IKanaRepository
{
	public async Task<Kana?> GetByCharacterAsync(string questionText, CancellationToken cancellationToken = default)
	{
		return await DatabaseSet.FirstOrDefaultAsync(x => x.Character == questionText, cancellationToken);
	}

	public async Task<Kana?> GetRandomAsync(KanaType kanaType, CancellationToken cancellationToken = default)
	{
		return await DatabaseSet
			.Where(x => x.Type == kanaType)
			.OrderBy(x => Guid.NewGuid())
			.FirstOrDefaultAsync(cancellationToken);
	}

	public async Task<List<Kana>> GetWrongAnswersAsync(string correctAnswer, KanaType kanaType, int count, CancellationToken cancellationToken = default)
	{
		// Get all kana of the same type except the correct answer
		List<Kana> allKana = await DatabaseSet
			.Where(x => x.Type == kanaType && x.Romaji != correctAnswer)
			.ToListAsync(cancellationToken);

		if (allKana.Count == 0)
			return new List<Kana>();

		// Find similar answers (same first character or similar sounds)
		List<Kana> similarKana = allKana
			.Where(k => k.Romaji.StartsWith(correctAnswer[0]) || 
			           CalculateSimilarity(k.Romaji, correctAnswer) > 0.5)
			.ToList();

		// Find dissimilar answers
		List<Kana> dissimilarKana = allKana
			.Where(k => !similarKana.Contains(k))
			.ToList();

		List<Kana> result = new List<Kana>();
		
		// Add one similar answer if available
		if (similarKana.Count > 0)
		{
			Random random = new Random();
			result.Add(similarKana[random.Next(similarKana.Count)]);
		}

		// Fill remaining slots with dissimilar answers
		int remaining = count - result.Count;
		if (dissimilarKana.Count > 0 && remaining > 0)
		{
			Random random = new Random();
			List<Kana> selectedDissimilar = dissimilarKana
				.OrderBy(x => random.Next())
				.Take(remaining)
				.ToList();
			result.AddRange(selectedDissimilar);
		}

		// If we still need more options, add from any remaining kana
		if (result.Count < count && allKana.Count > result.Count)
		{
			Random random = new Random();
			List<Kana> remainingKana = allKana
				.Where(k => !result.Contains(k))
				.OrderBy(x => random.Next())
				.Take(count - result.Count)
				.ToList();
			result.AddRange(remainingKana);
		}

		return result;
	}

	private static double CalculateSimilarity(string s1, string s2)
	{
		if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
			return 0;

		// Simple similarity based on common characters
		int commonChars = s1.Intersect(s2).Count();
		int maxLength = Math.Max(s1.Length, s2.Length);
		return (double)commonChars / maxLength;
	}
}
