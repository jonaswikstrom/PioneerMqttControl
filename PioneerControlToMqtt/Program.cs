using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using PioneerControlToMqtt.MessageHandlers;
using PioneerControlToMqtt.Mqtt;

namespace PioneerControlToMqtt
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting host for PioneerControlToMqtt");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, true)
                .AddEnvironmentVariables()
                .Build();

            var hostBuilder = new HostBuilder().ConfigureServices(p =>
            {
                p.AddLogging(p => p.AddConsole())
                    .Configure<MqttSettings>(configuration.GetSection("Mqtt"))
                    .Configure<PioneerControlSettings>(configuration.GetSection("PioneerReceiver"))
                    .AddSingleton<IMqttClient, MqttClient>()
                    .AddSingleton<IPioneerConnection, PioneerConnection>()
                    .AddSingleton<IMessageHandler, HeartbeatMessageHandler>()
                    .AddSingleton<IMessageHandler, PowerMessageHandler>()
                    .AddSingleton<IMessageHandler, VolumeMessageHandler>()
                    .AddSingleton<IMessageHandler, InputMessageHandler>()
                    .AddHostedService<PioneerControlHost>()
                    .AddTransient(provider => new Lazy<IPioneerConnection>(provider.GetService<IPioneerConnection>));
            });

            await hostBuilder.RunConsoleAsync();
        }
    }
}
