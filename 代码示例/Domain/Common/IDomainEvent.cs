using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Domain/Common/IDomainEvent.cs
using MediatR;

namespace Domain.Common
{
    public interface IDomainEvent : INotification
    {
        DateTime OccurredOn { get; }
    }
}