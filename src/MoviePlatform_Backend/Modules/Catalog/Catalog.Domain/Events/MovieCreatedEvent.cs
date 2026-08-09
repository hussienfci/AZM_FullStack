using System;
using MoviePlatform.Shared.Kernel.Entities;

namespace MoviePlatform.Modules.Catalog.Domain.Events;

public record MovieCreatedEvent(Guid MovieId, string Title) : DomainEvent;
