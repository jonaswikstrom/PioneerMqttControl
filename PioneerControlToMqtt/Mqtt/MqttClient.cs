using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;

namespace PioneerControlToMqtt.Mqtt
{
    public class MqttClient : IMqttClient
    {
        private readonly ILogger<MqttClient> logger;
        private readonly IOptions<MqttSettings> settings;
        private readonly IHostApplicationLifetime applicationLifetime;
        private readonly MQTTnet.Client.IMqttClient client;
        private readonly IMqttClientOptions options;
        private readonly ConcurrentDictionary<string, Func<string, Task>> commandActions = new ConcurrentDictionary<string, Func<string, Task>>();

        public MqttClient(ILogger<MqttClient> logger, IOptions<MqttSettings> settings, IHostApplicationLifetime applicationLifetime)
        {
            this.logger = logger;
            this.settings = settings;
            this.applicationLifetime = applicationLifetime;
            var factory = new MQTTnet.MqttFactory();
            client = factory.CreateMqttClient();

            options = new MqttClientOptionsBuilder()
                .WithTcpServer(settings.Value.HostName)
                .WithCredentials(settings.Value.Username, settings.Value.Password)
                .Build();
        }

        public async Task ConnectAsync()
        {
            logger.LogInformation($"Connecting MQTT on {settings.Value.HostName}");

            await client.ConnectAsync(options, CancellationToken.None);
            client.UseApplicationMessageReceivedHandler(async message =>
            {
                var payload = message.ApplicationMessage.ConvertPayloadToString();
                logger.LogInformation($"Received {message.ApplicationMessage.Topic} {payload}");

                if (commandActions.TryGetValue(message.ApplicationMessage.Topic, out var command))
                    await command(payload);
            });

            client.UseDisconnectedHandler(p =>
            {
                logger.LogInformation("MQTT disconnected, terminating application");
                applicationLifetime.StopApplication();
            });

            logger.LogInformation("Connected");
        }

        public async Task SubscribeCommandTopicAsync(string topic, Func<string, Task> commandAction)
        {
            if (commandActions.ContainsKey($"{settings.Value.TopicRoot}/{ topic}")) return;

            await client.SubscribeAsync(new TopicFilterBuilder().WithTopic($"{settings.Value.TopicRoot}/{topic}").Build());
            commandActions.TryAdd($"{settings.Value.TopicRoot}/{ topic}", commandAction);
            logger.LogInformation($"Subscribed to topic '{settings.Value.TopicRoot}/{topic}'");
        }

        public async Task PublishAsync(string topic, string payload)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic($"{settings.Value.TopicRoot}/{topic}")
                .WithPayload(payload)
                .WithRetainFlag()
                .Build();

            await client.PublishAsync(message);
            logger.LogInformation($"Published '{settings.Value.TopicRoot}/{topic} {payload}'");
        }

        public async Task DisconnectAsync()
        {
            if (client == null) return;
            if (!client.IsConnected) return;
            await client.DisconnectAsync();
        }

        public void Dispose()
        {
            client?.Dispose();
        }
    }
}