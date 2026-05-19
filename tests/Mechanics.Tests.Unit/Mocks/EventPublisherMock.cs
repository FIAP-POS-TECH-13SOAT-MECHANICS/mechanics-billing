using Mechanics.Infra.Messaging.Publishers;

namespace Mechanics.Tests.Unit.Mocks;

public class EventPublisherMock : IEventPublisher
{
    public List<object> PublishedMessages { get; } = [];

    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        PublishedMessages.Add(message);
        return Task.CompletedTask;
    }
}
