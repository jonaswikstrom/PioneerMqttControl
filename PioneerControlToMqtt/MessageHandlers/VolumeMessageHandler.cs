﻿using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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

        public VolumeMessageHandler(ILogger<VolumeMessageHandler> logger, IConfiguration configuration, 
            Lazy<IPioneerConnection> pioneerConnection, IMqttClient mqttClient) : base(configuration, mqttClient)
        {
            this.logger = logger;
            this.pioneerConnection = pioneerConnection;
            this.mqttClient = mqttClient;
        }

        protected override Regex Regex => new Regex(@"^VOL\d\d\d$");
        protected override async Task DoHandleMessage(string message)
        {
            if (volumeChange) return;

            var volume = int.Parse(message.Replace("VOL", ""));
            currentVolume = volume;
            logger.LogInformation($"Volume: {volume}");

            await mqttClient.PublishAsync($"{Topic}", volume.ToString());
        }

        protected override string Topic => "volume";

        protected override async Task OnCommand(string payload)
        {
            if (!int.TryParse(payload, out var volume)) return;
            if (volume > MaxVolume || volume < 0) return;

            volumeChange = true;
            currentVolume = null;

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            var connection = pioneerConnection.Value;
            await connection.SendCommandAsync(PioneerCommand.VolumeInfo);

            do
            {
                await Task.Delay(300, cts.Token);
            } while (!cts.IsCancellationRequested && !currentVolume.HasValue);

            if (!currentVolume.HasValue)
            {
                logger.LogError("Current volume not set, exiting");
                volumeChange = false;
                return;
            }

            if (currentVolume.Value == volume) return;
            cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var volumeCommand = currentVolume < volume ? PioneerCommand.VolumeUp : PioneerCommand.VolumeDown;
            do
            {
                await connection.SendCommandAsync(volumeCommand);
                await Task.Delay(300, CancellationToken.None);

                if (volumeCommand == PioneerCommand.VolumeDown && currentVolume <= volume) break;
                if (volumeCommand == PioneerCommand.VolumeUp && currentVolume >= volume) break;

                if (currentVolume == volume) break;
                if (currentVolume >= MaxVolume) break;
            } while (!cts.IsCancellationRequested);

            volumeChange = false;
            await connection.SendCommandAsync(PioneerCommand.VolumeInfo);
        }
    }
}