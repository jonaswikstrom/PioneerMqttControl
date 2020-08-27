using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PioneerControlToMqtt.Mqtt;

namespace PioneerControlToMqtt.MessageHandlers
{
    public class InputMessageHandler : MessageHandler
    {
        private readonly ILogger logger;
        private readonly Lazy<IPioneerConnection> pioneerConnection;
        private readonly IMqttClient mqttClient;

        public InputMessageHandler(ILogger<InputMessageHandler> logger, IOptions<MqttSettings> settings, Lazy<IPioneerConnection> pioneerConnection,
            IMqttClient mqttClient) : base(settings, mqttClient)
        {
            this.logger = logger;
            this.pioneerConnection = pioneerConnection;
            this.mqttClient = mqttClient;
        }

        protected override Regex Regex => new Regex(@"FN\d\d");
        protected override async Task DoHandleMessage(string message)
        {
            var device = InputSelection.Parse(message);
            logger.LogInformation($"Device {device.Number} {device.Name}");

            var idTask = mqttClient.PublishAsync($"{Topic}/id", device.Number.ToString());
            var nameTask = mqttClient.PublishAsync($"{Topic}/name", device.Name);

            await Task.WhenAll(idTask, nameTask);
        }

        protected override string Topic => "device";
        protected override async Task OnCommand(string payload)
        {
            if (!int.TryParse(payload, out var result)) return;
            if (result < 0 || result > 99) return;

            var command = $"{result.ToString().PadLeft(2, '0')}{PioneerCommand.FunctionChange}";
            await pioneerConnection.Value.SendCommandAsync(command);
        }
    }
}