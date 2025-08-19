using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasks.Domain.Interfaces;

namespace Tasks.Domain.Common
{
	public abstract class EntityBase
	{
		public Guid Id { get; protected set; } = Guid.NewGuid();

		private readonly List<IDomainEvent> _domainEvents = new();
		public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

		protected void AddDomainEvent(IDomainEvent eventItem)
		{
			_domainEvents.Add(eventItem);
		}

		public void ClearDomainEvents()
		{
			_domainEvents.Clear();
		}

		public override bool Equals(object? obj)
		{
			if (obj is not EntityBase other) return false;
			if (ReferenceEquals(this, other)) return true;
			return Id.Equals(other.Id);
		}

		public override int GetHashCode() => Id.GetHashCode();
	}

}
