using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PioneerControlToMqtt.Mqtt;

namespace PioneerControlToMqtt.MessageHandlers
{
    public class HeartbeatMessageHandler : MessageHandler
    {
        private readonly ILogger<HeartbeatMessageHandler> logger;
        private readonly IMqttClient mqttClient;

        public HeartbeatMessageHandler(ILogger<HeartbeatMessageHandler> logger, IConfiguration configuration, IMqttClient mqttClient)
            : base(configuration, mqttClient)
        {
            this.logger = logger;
            this.mqttClient = mqttClient;
        }

        protected override Regex Regex => new Regex(@"^R$");
        protected override async Task DoHandleMessage(string message)
        {
            var now = DateTime.Now;
            logger.LogInformation($"Hartbeat {now}");
            await mqttClient.PublishAsync($"{Topic}", now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        protected override string Topic => "heartbeat";
        protected override async Task OnCommand(string payload)
        {
            await DoHandleMessage(payload);
        }
    }
}