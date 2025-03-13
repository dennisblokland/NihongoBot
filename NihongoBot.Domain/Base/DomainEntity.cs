namespace NihongoBot.Domain.Base
{
	public abstract class DomainEntity<T>
	{
		public T Id { get; set; }
		public DateTime? CreatedAt { get; private set; }
		public DateTime? UpdatedAt { get; private set; }

		protected DomainEntity()
		{

		}

		public void UpdateTimestamps()
		{
			UpdatedAt = DateTime.UtcNow;
			if (CreatedAt == null)
			{
				CreatedAt = DateTime.UtcNow;
			}
		}
	}

	public abstract class DomainEntity : DomainEntity<Guid>
	{
		protected DomainEntity() : base()
		{
			Id = Guid.NewGuid();
		}
	}
}
