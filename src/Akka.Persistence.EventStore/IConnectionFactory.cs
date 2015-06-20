using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;

namespace Akka.Persistence.EventStore
{
    public interface IConnectionFactory
    {
        ConnectionSettings CreateConnectionSettings();
        Task<IEventStoreConnection> CreateAsync(string host, int tcpPort);
    }

    public class DefaultConnectionFactory : IConnectionFactory
    {
        public virtual ConnectionSettings CreateConnectionSettings()
        {
            return ConnectionSettings.Create()
                .KeepReconnecting()
                .UseConsoleLogger()
                .SetDefaultUserCredentials(new UserCredentials("admin", "changeit"));
        }

        public async Task<IEventStoreConnection> CreateAsync(string host, int tcpPort)
        {
            var address = (await Dns.GetHostAddressesAsync(host)).First();
            var connection = EventStoreConnection.Create(CreateConnectionSettings(), new IPEndPoint(address, tcpPort));
            await connection.ConnectAsync();
            return connection;
        }
    }
}