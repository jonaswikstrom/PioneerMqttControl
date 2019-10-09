using System;

namespace PioneerControlToMqtt
{
    public class Configuration : IConfiguration
    {
        public string GetEnvironmentVariable(string key, string defaultValue = null)
        {
            var value = Environment.GetEnvironmentVariable(key);
            return string.IsNullOrWhiteSpace(value) ? default : value;
        }

        private string HostName => GetEnvironmentVariable("RECEIVERHOST");

        private string Port => GetEnvironmentVariable("RECEIVERPORT");

        public PioneerConnectionInfo ConnectionInfo => new PioneerConnectionInfo(HostName, int.Parse(Port));
        public string MqttHostName => GetEnvironmentVariable("MQTTHOST");
        public string MqttUsername => GetEnvironmentVariable("MQTTUSERNAME");
        public string MqttPassword => GetEnvironmentVariable("MQTTPASSWORD");
        public string TopicRoot => GetEnvironmentVariable("TOPICROOT", string.Empty);
        public TimeSpan ConnectionTimeOut { get; } = TimeSpan.FromMinutes(1);
    }
}