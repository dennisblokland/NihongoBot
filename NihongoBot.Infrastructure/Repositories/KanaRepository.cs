
using Microsoft.EntityFrameworkCore;

using Name.Infrastructure.Repositories;

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
}
