using System;
using MoviePlatform.Shared.Kernel.Entities;

namespace MoviePlatform.Modules.Catalog.Domain.Events;

public record MovieCreatedEvent : DomainEvent
{
    public Guid MovieId { get; init; }
    public string Title { get; init; }

    public MovieCreatedEvent(Guid movieId, string title, DateTime occurredOn)
    {
        MovieId = movieId;
        Title = title;
        _ = occurredOn;
    }
}