using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PioneerControlToMqtt.Mqtt;

namespace PioneerControlToMqtt.MessageHandlers
{
    public class HeartbeatMessageHandler : MessageHandler
    {
        private readonly ILogger<HeartbeatMessageHandler> logger;
        private readonly IOptions<PioneerControlSettings> pioneerControlSettings;
        private readonly IMqttClient mqttClient;
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private Timer timer;

        public HeartbeatMessageHandler(ILogger<HeartbeatMessageHandler> logger, IOptions<MqttSettings> mqttSettings, IOptions<PioneerControlSettings> pioneerControlSettings, 
            IMqttClient mqttClient, IHostApplicationLifetime hostApplicationLifetime)
            : base(mqttSettings, mqttClient)
        {
            this.logger = logger;
            this.pioneerControlSettings = pioneerControlSettings;
            this.mqttClient = mqttClient;
            this.hostApplicationLifetime = hostApplicationLifetime;
            timer = new Timer(pioneerControlSettings.Value.HeartbeatTimeout.TotalMilliseconds);
            timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            logger.LogInformation("Heartbeat timeout occured. Terminating the application");
            hostApplicationLifetime.StopApplication();
        }

        protected override Regex Regex => new Regex(@"^R$");
        protected override async Task DoHandleMessage(string message)
        {
            timer.Stop();
            timer.Start();
            var now = DateTime.Now;
            logger.LogInformation($"Receiver heartbeat {now}");
            await mqttClient.PublishAsync($"{Topic}", now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        protected override string Topic => "heartbeat";
        protected override async Task OnCommand(string payload)
        {
            await DoHandleMessage(payload);
        }
    }
}