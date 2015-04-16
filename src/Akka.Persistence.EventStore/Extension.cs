using System;
using Akka.Actor;
using Akka.Configuration;

namespace Akka.Persistence.EventStore
{
    /// <summary>
    /// Configuration settings representation targeting Sql Server journal actor.
    /// </summary>
    public class JournalSettings
    {
        public const string ConfigPath = "akka.persistence.journal.eventstore";

        /// <summary>
        /// Connection string used to access a persistent EventStore instance.
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// Connection timeout for EventStore related operations.
        /// </summary>
        public int TcpPort { get; private set; }

        /// <summary>
        /// Connection settings for EventStore connections.
        /// </summary>
        public IConnectionSettingsFactory ConnectionSettingsFactory { get; private set; }

        public JournalSettings(Config config)
        {
            if (config == null) throw new ArgumentNullException("config", "EventStore journal settings cannot be initialized, because required HOCON section couldn't been found");

            Host = config.GetString("server-host");
            TcpPort = config.GetInt("server-tcp-port");

            var settingsFactoryType = Type.GetType(config.GetString("connection-settings-factory"));
            ConnectionSettingsFactory = settingsFactoryType != null
                ? (IConnectionSettingsFactory) Activator.CreateInstance(settingsFactoryType)
                : new DefaultConnectionSettingsFactory();
        }
    }

    /// <summary>
    /// Configuration settings representation targeting Sql Server snapshot store actor.
    /// </summary>
    public class SnapshotStoreSettings
    {
        public const string ConfigPath = "akka.persistence.snapshot-store.eventstore";

        /// <summary>
        /// Connection string used to access a persistent EventStore instance.
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// Connection timeout for EventStore related operations.
        /// </summary>
        public int TcpPort { get; private set; }

        /// <summary>
        /// Connection settings for EventStore connections.
        /// </summary>
        public IConnectionSettingsFactory ConnectionSettingsFactory { get; private set; }

        public SnapshotStoreSettings(Config config)
        {
            if (config == null) throw new ArgumentNullException("config", "EventStore journal settings cannot be initialized, because required HOCON section couldn't been found");

            Host = config.GetString("server-host");
            TcpPort = config.GetInt("server-tcp-port");

            var settingsFactoryType = Type.GetType(config.GetString("connection-settings-factory"));
            ConnectionSettingsFactory = settingsFactoryType != null
                ? (IConnectionSettingsFactory)Activator.CreateInstance(settingsFactoryType)
                : new DefaultConnectionSettingsFactory();
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
        public readonly SnapshotStoreSettings SnapshotStoreSettings;

        public EventStorePersistenceExtension(ExtendedActorSystem system)
        {
            system.Settings.InjectTopLevelFallback(EventStorePersistence.DefaultConfiguration());

            JournalSettings = new JournalSettings(system.Settings.Config.GetConfig(JournalSettings.ConfigPath));
            SnapshotStoreSettings = new SnapshotStoreSettings(system.Settings.Config.GetConfig(SnapshotStoreSettings.ConfigPath));
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