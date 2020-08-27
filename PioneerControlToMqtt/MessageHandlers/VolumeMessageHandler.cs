using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PioneerControlToMqtt.Mqtt;

namespace PioneerControlToMqtt.MessageHandlers
{
    public class VolumeMessageHandler : MessageHandler
    {
        private const int MaxVolume = 120;
        private readonly ILogger<VolumeMessageHandler> logger;
        private readonly Lazy<IPioneerConnection> pioneerConnection;
        private readonly IMqttClient mqttClient;
        private int? currentVolume;
        private bool volumeChange;

        public VolumeMessageHandler(ILogger<VolumeMessageHandler> logger, IOptions<MqttSettings> settings, 
            Lazy<IPioneerConnection> pioneerConnection, IMqttClient mqttClient) : base(settings, mqttClient)
        {
            this.logger = logger;
            this.pioneerConnection = pioneerConnection;
            this.mqttClient = mqttClient;
        }

        protected override Regex Regex => new Regex(@"^VOL\d\d\d$");
        protected override async Task DoHandleMessage(string message)
        {
            var volume = int.Parse(message.Replace("VOL", ""));
            currentVolume = volume;
            logger.LogInformation($"Volume: {volume}");

            if (volumeChange)
            {
                logger.LogInformation("Volume is changing, no publish is made");
                return;
            }

            await mqttClient.PublishAsync($"{Topic}", volume.ToString());
        }

        protected override string Topic => "volume";

        protected override async Task OnCommand(string payload)
        {
            if (!int.TryParse(payload, out var volume)) return;
            if (volume > MaxVolume || volume < 0) return;
            currentVolume = null;

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            var connection = pioneerConnection.Value;
            await connection.SendCommandAsync(PioneerCommand.VolumeInfo);

            do
            {
                await Task.Delay(500, CancellationToken.None);
            } while (!cts.IsCancellationRequested && !currentVolume.HasValue);

            if (!currentVolume.HasValue)
            {
                logger.LogInformation("Current volume not set, exiting");
                volumeChange = false;
                return;
            }

            if (currentVolume.Value == volume) return;

            volumeChange = true;
            cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var volumeCommand = currentVolume < volume ? PioneerCommand.VolumeUp : PioneerCommand.VolumeDown;
            do
            {
                await connection.SendCommandAsync(volumeCommand);
                await Task.Delay(500, CancellationToken.None);

                if (volumeCommand == PioneerCommand.VolumeDown && currentVolume <= volume) break;
                if (volumeCommand == PioneerCommand.VolumeUp && currentVolume >= volume) break;

                if (currentVolume == volume) break;
                if (currentVolume >= MaxVolume) break;
            } while (!cts.IsCancellationRequested);

            volumeChange = false;
            await Task.Delay(200, CancellationToken.None);
            await connection.SendCommandAsync(PioneerCommand.VolumeInfo);
        }
    }
}