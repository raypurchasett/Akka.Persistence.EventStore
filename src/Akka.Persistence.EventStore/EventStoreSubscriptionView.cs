using System;
using Akka.Actor;
using Akka.Event;
using Akka.Persistence.EventStore.Journal;
using Akka.Serialization;
using EventStore.ClientAPI;
using Nito.AsyncEx;

namespace Akka.Persistence.EventStore
{
    public abstract class EventStoreSubscriptionView : PersistentView
    {
        private readonly IDeserializer _deserializer;
        private readonly Serializer _serializer;
        private IActorRef _self;
        private readonly IEventStoreConnection _connection;
        private readonly ILoggingAdapter _log;

        protected EventStoreSubscriptionView()
        {
            _log = Context.GetLogger();

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

        private void SubscriptionDropped(EventStoreSubscription subscription, SubscriptionDropReason reason, Exception exception)
        {
            _log.Error(exception, "Subscription dropped. Reason: {0}", reason);
            SubscribeToStream();
        }

        public override bool IsAutoUpdate { get { return false; } }

        protected override void PreStart()
        {
            base.PreStart();

            _self = Self;

            SubscribeToStream();
        }

        private void SubscribeToStream()
        {
            AsyncContext.Run(
                async () =>
                {
                    _log.Info("Subscribing to stream: {0}", PersistenceId);
                    await _connection.SubscribeToStreamAsync(PersistenceId, false, EventAppeard, SubscriptionDropped);
                });
        }
    }
}