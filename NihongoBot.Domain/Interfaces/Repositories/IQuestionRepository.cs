
using NihongoBot.Domain.Entities;

namespace NihongoBot.Domain.Interfaces.Repositories;

public interface IQuestionRepository : IDomainRepository<Question, Guid>
{
	Task<IEnumerable<Question>> GetExpiredQuestionsAsync(CancellationToken cancellationToken = default);
	Task<Question?> GetOldestUnansweredQuestionAsync(Guid id, CancellationToken cancellationToken = default);
}
