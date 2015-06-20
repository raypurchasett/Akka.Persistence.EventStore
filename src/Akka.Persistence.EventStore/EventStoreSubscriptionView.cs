using Akka.Actor;
using Akka.Persistence.EventStore.Journal;
using Akka.Serialization;
using EventStore.ClientAPI;
using Nito.AsyncEx;

namespace Akka.Persistence.EventStore
{
    /// <summary>
    /// A <see cref="PersistentView"/> that uses an <see cref="EventStoreSubscription"/> to stay up to date instead of an <see cref="PersistentView.AutoUpdateInterval"/>.
    /// </summary>
    public abstract class EventStoreSubscriptionView : PersistentView
    {
        private readonly IDeserializer _deserializer;
        private readonly Serializer _serializer;
        private IActorRef _self;
        private readonly IEventStoreConnection _connection;

        protected EventStoreSubscriptionView()
        {
            var extension = EventStorePersistence.Instance.Apply(Context.System);
            _connection = extension.ServerSettings.Connection;

            _deserializer = extension.JournalSettings.Deserializer;
            _serializer = Context.System.Serialization.FindSerializerForType(typeof(IPersistentRepresentation));
        }

        private void EventAppeard(EventStoreSubscription subscription, ResolvedEvent @event)
        {
            var representation = _deserializer.GetRepresentation(_serializer, @event.OriginalEvent);
            var payload = representation.Payload;
            if (payload != null)
            {
                _self.Tell(payload);
            }
        }

        public override bool IsAutoUpdate { get { return false; } }

        protected override void PreStart()
        {
            base.PreStart();

            _self = Self;

            AsyncContext.Run(async () =>
            {
                await _connection.SubscribeToStreamAsync(PersistenceId, false, EventAppeard);
            });
        }
    }
}