using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PioneerControlToMqtt.Mqtt;

namespace PioneerControlToMqtt
{
    public class PioneerControlHost : IHostedService
    {
        private readonly IMqttClient mqttClient;
        private readonly ILogger<PioneerControlHost> logger;
        private readonly IHostApplicationLifetime applicationLifetime;
        private readonly IPioneerConnection pioneerConnection;

        public PioneerControlHost(ILogger<PioneerControlHost> logger, IHostApplicationLifetime applicationLifetime, IPioneerConnection pioneerConnection, IMqttClient mqttClient)
        {
            this.mqttClient = mqttClient;
            this.logger = logger;
            this.applicationLifetime = applicationLifetime;
            this.pioneerConnection = pioneerConnection;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            applicationLifetime.ApplicationStarted.Register(async () => await OnStarted());
            return Task.CompletedTask;
        }

        private async Task OnStarted()
        {
            await mqttClient.ConnectAsync();
            await pioneerConnection.ConnectAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Disconnecting and disposing");
            await mqttClient.DisconnectAsync();
            mqttClient.Dispose();
            pioneerConnection.Dispose();
        }
    }
}