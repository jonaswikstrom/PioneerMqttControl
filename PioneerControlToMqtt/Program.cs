using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using PioneerControlToMqtt.MessageHandlers;
using PioneerControlToMqtt.Mqtt;

namespace PioneerControlToMqtt
{
    class Program
    {

        static async Task Main(string[] args)
        {
            var hostBuilder = new HostBuilder().ConfigureServices(p =>
            {
                p.AddLogging(p => p.AddConsole())
                    .AddSingleton<IConfiguration, Configuration>()
                    .AddSingleton<Mqtt.IMqttClient, Mqtt.MqttClient>()
                    .AddSingleton<IPioneerConnection, PioneerConnection>()
                    .AddSingleton<IMessageHandler, HeartbeatMessageHandler>()
                    .AddSingleton<IMessageHandler, PowerMessageHandler>()
                    .AddSingleton<IMessageHandler, VolumeMessageHandler>()
                    .AddSingleton<IMessageHandler, InputMessageHandler>()
                    .AddHostedService<PioneerControlHost>()
                    .AddTransient(provider => new Lazy<IPioneerConnection>(provider.GetService<IPioneerConnection>));
            });

            await hostBuilder.RunConsoleAsync();
        }
    }

    public class PioneerControlHost : IHostedService
    {
        private readonly IMqttClient mqttClient;
        private readonly ILogger<PioneerControlHost> logger;
        private readonly IPioneerConnection pioneerConnection;

        public PioneerControlHost(ILogger<PioneerControlHost> logger, IPioneerConnection pioneerConnection, Mqtt.IMqttClient mqttClient)
        {
            this.mqttClient = mqttClient;
            this.logger = logger;
            this.pioneerConnection = pioneerConnection;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await mqttClient.ConnectAsync();
            await pioneerConnection.ConnectAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await mqttClient.DisconnectAsync();
            mqttClient.Dispose();
            pioneerConnection.Dispose();
        }
    }
}
