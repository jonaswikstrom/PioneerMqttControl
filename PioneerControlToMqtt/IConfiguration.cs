using System;

namespace PioneerControlToMqtt
{
    public interface IConfiguration
    {
        TimeSpan ConnectionTimeOut { get; }
        PioneerConnectionInfo ConnectionInfo { get; }

        string MqttHostName { get; }
        string MqttUsername { get; }
        string MqttPassword { get; }

        string TopicRoot { get; }
    }
}