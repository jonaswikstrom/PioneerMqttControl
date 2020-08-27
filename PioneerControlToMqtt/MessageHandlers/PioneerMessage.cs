using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PioneerControlToMqtt.Mqtt;

namespace PioneerControlToMqtt.MessageHandlers
{
    public abstract class MessageHandler : IMessageHandler
    {
        private readonly IMqttClient mqttClient;

        protected MessageHandler(IOptions<MqttSettings> settings, IMqttClient mqttClient)
        {
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

        public async Task SubscribeToCommandTopic()
        {
            await mqttClient.SubscribeCommandTopicAsync($"{Topic}/cmnd", OnCommand);
        }

        protected abstract Regex Regex { get; }
        protected abstract string Topic { get; }

        protected abstract Task DoHandleMessage(string message);


        protected abstract Task OnCommand(string payload);
    }
}