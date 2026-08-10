using System;
using MoviePlatform.Shared.Kernel.Entities;

namespace MoviePlatform.Modules.Catalog.Contracts.Events;

public record MovieCreatedIntegrationEvent(Guid MovieId, string Title, DateTime OccurredOn) : DomainEvent;
