using System;
using System.Threading.Tasks;

namespace PioneerControlToMqtt.Mqtt
{
    public interface IMqttClient : IDisposable
    {
        Task DisconnectAsync();

        Task PublishAsync(string topic, string payload);
        Task ConnectAsync();

        Task SubscribeCommandTopicAsync(string topic, Func<string, Task> commandAction);
    }
}