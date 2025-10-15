using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Common
{
    public abstract class Entity
    {
        private List<IDomainEvent> _domainEvents;

        [NotMapped] // 确保不会被EF Core持久化
        public IReadOnlyCollection<IDomainEvent> DomainEvents =>
            _domainEvents?.AsReadOnly() ?? new List<IDomainEvent>().AsReadOnly();

        public void AddDomainEvent(IDomainEvent eventItem)
        {
            _domainEvents ??= new List<IDomainEvent>();
            _domainEvents.Add(eventItem);
        }

        public void RemoveDomainEvent(IDomainEvent eventItem)
        {
            _domainEvents?.Remove(eventItem);
        }

        public void ClearDomainEvents()
        {
            _domainEvents?.Clear();
        }
    }
}