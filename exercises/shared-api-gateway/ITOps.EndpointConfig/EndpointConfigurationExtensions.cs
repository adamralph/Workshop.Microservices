using System;
using System.IO;
using NServiceBus;
using NServiceBus.Logging;

namespace ITOps.EndpointConfig
{
    public static class EndpointConfigurationExtensions
    {
        static readonly ILog Log = LogManager.GetLogger(typeof(EndpointConfigurationExtensions));

        public static EndpointConfiguration Configure(
            this EndpointConfiguration endpointConfiguration)
        {
            Log.Info("Configuring endpoint...");

            var licensePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\..\\..\\..\\License.xml");
            endpointConfiguration.LicensePath(licensePath);
            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            endpointConfiguration.Recoverability().Delayed(c => c.NumberOfRetries(0));

            var transport = endpointConfiguration.UseTransport<LearningTransport>();

            var routing = transport.Routing();

            var persistence = endpointConfiguration.UsePersistence<LearningPersistence>();

            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.AuditProcessedMessagesTo("audit");

            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningCommandsAs(t => t.Namespace != null && t.Namespace.StartsWith("Divergent") && t.Namespace.EndsWith("Commands") && t.Name.EndsWith("Command"));
            conventions.DefiningEventsAs(t => t.Namespace != null && t.Namespace.StartsWith("Divergent") && t.Namespace.EndsWith("Events") && t.Name.EndsWith("Event"));

            endpointConfiguration.EnableInstallers();

            return endpointConfiguration;
        }
    }
}
