using System;
using System.Net;
using Akka.Configuration;
using Akka.Persistence.TestKit.Journal;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Embedded;
using EventStore.Core;

namespace Akka.Persistence.EventStore.Tests
{
    public class EventStoreJournalSpec : JournalSpec
    {
        private static readonly Config SpecConfig = ConfigurationFactory.ParseString(@"
        akka.persistence {
            publish-plugin-commands = on
            journal {
                plugin = ""akka.persistence.journal.eventstore""
                eventstore {
                    class = ""Akka.Persistence.EventStore.Journal.EventStoreJournal, Akka.Persistence.EventStore""
                    plugin-dispatcher = ""akka.actor.default-dispatcher""
                    server-host = ""127.0.0.1""
                    server-tcp-port = 4532
                    connection-settings-factory = ""Akka.Persistence.EventStore.DefaultConnectionSettingsFactory, Akka.Persistence.EventStore""
                }
            }
        }");

//        private static readonly ClusterVNode Node;

        //This shouldn't be necessary. The base ctor starts sending messages before I can spin up my ES node. BOO!
        static EventStoreJournalSpec()
        {
            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4532);
            //BUG: Not working anyway. use your running server.
//            Node = EmbeddedVNodeBuilder
//                .AsSingleNode()
//                .WithInternalTcpOn(tcpEndPoint)
//                .WithInternalHttpOn(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5553))
//                .Build();
//
//            Node.Start();
        }

        public EventStoreJournalSpec()
            : base(SpecConfig, "EventStoreJournalSpec")
        {}

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
//            Node.Stop();
        }
    }
}