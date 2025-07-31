
using Microsoft.EntityFrameworkCore;

using NihongoBot.Domain.Entities;
using NihongoBot.Domain.Interfaces.Repositories;

namespace NihongoBot.Infrastructure.Repositories;

public class QuestionRepository(IServiceProvider serviceProvider) : AbstractDomainRepository<Question, Guid>(serviceProvider), IQuestionRepository
{
	public async Task<IEnumerable<Question>> GetExpiredPendingAcceptanceQuestionsAsync(CancellationToken cancellationToken = default)
	{
		return await DatabaseSet
			.AsNoTracking()
			.Where(q =>
				!q.IsAnswered &&
				!q.IsExpired &&
				!q.IsAccepted &&
				q.CreatedAt.Value.AddHours(1) <= DateTime.UtcNow
			)
			.ToListAsync(cancellationToken);
	}

	public async Task<IEnumerable<Question>> GetExpiredQuestionsAsync(CancellationToken cancellationToken = default)
	{
		return await DatabaseSet
			.AsNoTracking()
			.Where(q =>
				!q.IsAnswered &&
				!q.IsExpired &&
				q.IsAccepted &&
				q.SentAt.AddMinutes(q.TimeLimit) <= DateTime.UtcNow
			)
			.ToListAsync(cancellationToken);
	}

	public async Task<Question?> GetOldestUnansweredQuestionAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await DatabaseSet
		.OrderBy(q => q.SentAt)
		.FirstOrDefaultAsync(q =>
			q.UserId == id &&
			!q.IsAnswered &&
			!q.IsExpired,
		cancellationToken
		);
	}
}
