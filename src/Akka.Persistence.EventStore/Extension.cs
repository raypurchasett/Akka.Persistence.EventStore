﻿using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Akka.Event;
using Akka.Persistence.EventStore.Journal;
using EventStore.ClientAPI;

namespace Akka.Persistence.EventStore
{
    public class ServerSettings
    {
        public const string ConfigPath = "akka.persistence.eventstore-server";

        private readonly Task<IEventStoreConnection> _init;

        public ServerSettings(ILoggingAdapter log, Config config)
        {
            if (config == null) throw new ArgumentNullException("config", "EventStore journal settings cannot be initialized, because required HOCON section couldn't been found");

            var host = config.GetString("host");
            var tcpPort = config.GetInt("tcp-port");

            var settingsFactoryType = Type.GetType(config.GetString("connection-factory"));
            var factory = settingsFactoryType != null
                ? (IConnectionFactory)Activator.CreateInstance(settingsFactoryType)
                : new DefaultConnectionFactory();

            _init = factory.CreateAsync(log, host, tcpPort);
        }

        public IEventStoreConnection Connection { get { return _init.Result; } }
    }

    /// <summary>
    /// Configuration settings representation targeting Sql Server journal actor.
    /// </summary>
    public class JournalSettings
    {
        public const string ConfigPath = "akka.persistence.journal.eventstore";


        public IDeserializer Deserializer { get; private set; }

        public JournalSettings(Config config)
        {
            if (config == null) throw new ArgumentNullException("config", "EventStore journal settings cannot be initialized, because required HOCON section couldn't been found");

            var deserializerType = Type.GetType(config.GetString("deserializer"));
            Deserializer = deserializerType != null
                ? (IDeserializer)Activator.CreateInstance(deserializerType)
                : new DefaultDeserializer();
        }
    }


    /// <summary>
    /// An actor system extension initializing support for EventStore persistence layer.
    /// </summary>
    public class EventStorePersistenceExtension : IExtension
    {
        /// <summary>
        /// Journal-related settings loaded from HOCON configuration.
        /// </summary>
        public readonly JournalSettings JournalSettings;

        /// <summary>
        /// Snapshot store related settings loaded from HOCON configuration.
        /// </summary>
        public readonly ServerSettings ServerSettings;

        public EventStorePersistenceExtension(ExtendedActorSystem system)
        {
            system.Settings.InjectTopLevelFallback(EventStorePersistence.DefaultConfiguration());

            JournalSettings = new JournalSettings(system.Settings.Config.GetConfig(JournalSettings.ConfigPath));
            ServerSettings = new ServerSettings(system.Log, system.Settings.Config.GetConfig(ServerSettings.ConfigPath));
        }
    }

    /// <summary>
    /// Singleton class used to setup EventStore backend for akka persistence plugin.
    /// </summary>
    public class EventStorePersistence : ExtensionIdProvider<EventStorePersistenceExtension>
    {
        public static readonly EventStorePersistence Instance = new EventStorePersistence();

        /// <summary>
        /// Initializes a EventStore persistence plugin inside provided <paramref name="actorSystem"/>.
        /// </summary>
        public static void Init(ActorSystem actorSystem)
        {
            Instance.Apply(actorSystem);
        }

        private EventStorePersistence() { }

        /// <summary>
        /// Creates an actor system extension for akka persistence EventStore support.
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        public override EventStorePersistenceExtension CreateExtension(ExtendedActorSystem system)
        {
            return new EventStorePersistenceExtension(system);
        }

        /// <summary>
        /// Returns a default configuration for akka persistence EventStore-based journals and snapshot stores.
        /// </summary>
        /// <returns></returns>
        public static Config DefaultConfiguration()
        {
            return ConfigurationFactory.FromResource<EventStorePersistence>("Akka.Persistence.EventStore.eventstore.conf");
        }
    }
}