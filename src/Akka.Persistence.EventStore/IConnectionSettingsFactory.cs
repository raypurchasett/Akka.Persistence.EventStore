using System;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;

namespace Akka.Persistence.EventStore
{
    public interface IConnectionSettingsFactory
    {
        ConnectionSettings Create();
    }

    public class DefaultConnectionSettingsFactory : IConnectionSettingsFactory
    {
        public ConnectionSettings Create()
        {
            return ConnectionSettings.Create()
                .KeepReconnecting()
                .UseConsoleLogger()
                .SetDefaultUserCredentials(new UserCredentials("admin", "changeit"));
        }
    }
}