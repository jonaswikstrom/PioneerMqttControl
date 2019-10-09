using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PioneerControlToMqtt.Mqtt;

namespace PioneerControlToMqtt.MessageHandlers
{
    public abstract class MessageHandler : IMessageHandler
    {
        private readonly ILogger logger;
        private readonly IMqttClient mqttClient;

        protected MessageHandler(ILogger logger, IConfiguration configuration, IMqttClient mqttClient)
        {
            this.logger = logger;
            this.mqttClient = mqttClient;
        }

        public async Task HandleReceiverMessage(string message)
        {
            if (!Regex.IsMatch(message)) return;
            await DoHandleMessage(Regex.Match(message).Value);
        }

        public async Task HandleMqttMessage()
        {
            await Task.Delay(1000);
        }

        protected abstract Regex Regex { get; }

        protected abstract Task DoHandleMessage(string message);

        protected abstract string Topic { get; }

        public async Task SubscribeToCommandTopic()
        {
            await mqttClient.SubscribeCommandTopicAsync($"{Topic}/cmnd", OnCommand);
        }

        protected abstract Task OnCommand(string payload);
    }
}