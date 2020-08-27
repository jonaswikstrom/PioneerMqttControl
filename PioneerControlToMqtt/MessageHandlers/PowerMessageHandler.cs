using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PioneerControlToMqtt.Mqtt;

namespace PioneerControlToMqtt.MessageHandlers
{
    public class PowerMessageHandler : MessageHandler
    {
        private readonly ILogger<PowerMessageHandler> logger;
        private readonly Lazy<IPioneerConnection> pioneerConnection;
        private readonly IMqttClient mqttClient;
        private PioneerPowerState? currentPowerState;
        
        public PowerMessageHandler(ILogger<PowerMessageHandler> logger, IOptions<MqttSettings> settings,
            Lazy<IPioneerConnection> pioneerConnection, IMqttClient mqttClient) 
            : base(settings, mqttClient)
        {
            this.logger = logger;
            this.pioneerConnection = pioneerConnection;
            this.mqttClient = mqttClient;
        }

        protected override Regex Regex => new Regex(@"^PWR\d$");
        protected override async Task DoHandleMessage(string message)
        {
            currentPowerState = message == "PWR0" ? PioneerPowerState.ON : PioneerPowerState.OFF;
            logger.LogInformation($"Power: {currentPowerState.ToString()}");

            await mqttClient.PublishAsync($"{Topic}", currentPowerState.ToString());
        }

        protected override string Topic => "power";

        protected override async Task OnCommand(string payload)
        {
            if (!Enum.TryParse<PioneerPowerState>(payload, true, out var powerstate)) return;

            var command = powerstate == PioneerPowerState.ON ? PioneerCommand.PowerOn : PioneerCommand.PowerOff;
            await pioneerConnection.Value.SendCommandAsync(command);
        }
    }
}
