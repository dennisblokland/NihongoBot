
using NihongoBot.Domain.Aggregates.Kana;
using NihongoBot.Domain.Enums;

namespace NihongoBot.Domain.Interfaces.Repositories;

public interface IKanaRepository : IDomainRepository<Kana, int>
{
	Task<Kana?> GetByCharacterAsync(string questionText, CancellationToken cancellationToken = default);
	Task<Kana?> GetRandomAsync(KanaType kanaType, CancellationToken cancellationToken = default);

}
