using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;

namespace PioneerControlToMqtt.Mqtt
{
    public class MqttClient : IMqttClient
    {
        private readonly ILogger<MqttClient> logger;
        private readonly IConfiguration configuration;
        private readonly MQTTnet.Client.IMqttClient client;
        private readonly IMqttClientOptions options;
        private readonly ConcurrentDictionary<string, Func<string, Task>> commandActions = new ConcurrentDictionary<string, Func<string, Task>>();

        public MqttClient(ILogger<MqttClient> logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
            var factory = new MQTTnet.MqttFactory();
            client = factory.CreateMqttClient();

            options = new MqttClientOptionsBuilder()
                .WithTcpServer(configuration.MqttHostName)
                .WithCredentials(configuration.MqttUsername, configuration.MqttPassword)
                .Build();
        }

        public async Task ConnectAsync()
        {
            logger.LogInformation($"Connecting MQTT on {configuration.MqttHostName}");

            await client.ConnectAsync(options, CancellationToken.None);
            client.UseApplicationMessageReceivedHandler(async message =>
            {
                var payload = message.ApplicationMessage.ConvertPayloadToString();
                logger.LogInformation($"Received {message.ApplicationMessage.Topic} {payload}");

                if (commandActions.TryGetValue(message.ApplicationMessage.Topic, out var command))
                    await command(payload);
            });

            logger.LogInformation("Connected");
        }

        public async Task SubscribeCommandTopicAsync(string topic, Func<string, Task> commandAction)
        {
            if (commandActions.ContainsKey($"{configuration.TopicRoot}/{ topic}")) return;

            await client.SubscribeAsync(new TopicFilterBuilder().WithTopic($"{configuration.TopicRoot}/{topic}").Build());
            commandActions.TryAdd($"{configuration.TopicRoot}/{ topic}", commandAction);
            logger.LogInformation($"Subscribed to topic '{configuration.TopicRoot}/{topic}'");
        }

        public async Task PublishAsync(string topic, string payload)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic($"{configuration.TopicRoot}/{topic}")
                .WithPayload(payload)
                .WithRetainFlag()
                .Build();

            await client.PublishAsync(message);
            logger.LogInformation($"Pubished '{configuration.TopicRoot}/{topic} {payload}'");
        }

        public async Task DisconnectAsync()
        {
            if (client == null) return;
            await client.DisconnectAsync();
        }

        public void Dispose()
        {
            client?.Dispose();
        }
    }
}