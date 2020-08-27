using System;
using System.Collections.Generic;
using System.Text;

namespace PioneerControlToMqtt.Mqtt
{
    public class MqttSettings
    {
        public string HostName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string TopicRoot { get; set; }
    }
}
