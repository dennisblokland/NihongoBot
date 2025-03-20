namespace NihongoBot.Domain.Interfaces;

public interface IDomainEntity
{
}
public interface IDomainEntity<out TKey> : IDomainEntity
		where TKey : struct
{
	TKey Id { get; }
}

