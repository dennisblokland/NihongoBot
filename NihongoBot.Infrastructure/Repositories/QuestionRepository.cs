
using Microsoft.EntityFrameworkCore;

using Name.Infrastructure.Repositories;

using NihongoBot.Domain;
using NihongoBot.Domain.Entities;
using NihongoBot.Domain.Interfaces.Repositories;

namespace NihongoBot.Infrastructure.Repositories;

public class QuestionRepository(IServiceProvider serviceProvider) : AbstractDomainRepository<Question, Guid>(serviceProvider), IQuestionRepository
{
	public async Task<IEnumerable<Question>> GetExpiredQuestionsAsync(CancellationToken cancellationToken = default)
	{
		return await DatabaseSet
			.Where(q => !q.IsAnswered && !q.IsExpired && q.SentAt.AddMinutes(q.TimeLimit) <= DateTime.UtcNow)
			.ToListAsync(cancellationToken);
	}

	public async Task<Question?> GetOldestUnansweredQuestionAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await DatabaseSet
		.OrderBy(q => q.SentAt)
		.FirstOrDefaultAsync(q =>
			q.UserId == id &&
			q.IsAnswered == false &&
			q.IsExpired == false,
		cancellationToken
		);
	}
}
