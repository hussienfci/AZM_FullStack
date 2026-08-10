using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MoviePlatform.Shared.Kernel.Entities;

namespace MoviePlatform.Shared.Infrastructure.Messaging;

public class MediatRDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;

    public MediatRDomainEventDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task DispatchAsync(AggregateRoot aggregate, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
        aggregate.ClearDomainEvents();
    }
}

public interface IDomainEventDispatcher
{
    Task DispatchAsync(AggregateRoot aggregate, CancellationToken cancellationToken = default);
}
