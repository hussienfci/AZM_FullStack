using System;
using Microsoft.EntityFrameworkCore;

namespace MoviePlatform.Shared.Kernel.Entities;

[Keyless]
public abstract record DomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
