using System;

namespace MoviePlatform.Shared.Kernel.Entities;

public abstract record DomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
